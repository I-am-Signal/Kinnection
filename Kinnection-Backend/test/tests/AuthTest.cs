using Kinnection;
using System.Net;
using System.Text.Json;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace test;

[TestFixture]
public class AuthTest
{
    private readonly string URI = TestRunner.GetURI();
    private readonly string UserSubDir = "users/";
    private readonly string AuthSubDir = "auth/";
    private Dictionary<string, JsonElement> UserInfo =
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
        @$"{{
            ""fname"": ""AuthFirst"",
            ""lname"": ""AuthLast"",
            ""email"": ""{Environment.GetEnvironmentVariable("MANUAL_EMAIL_VERIFICATION")}"",
            ""password"": ""{JsonSerializer.SerializeToElement(
                KeyMaster.Encrypt("TestPassword"))}""
        }}")!;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        // Make request
        var RequestContent = UserInfo;

        HttpResponseMessage Response = await HttpService.PostAsync(
            URI + UserSubDir,
            RequestContent
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        // Save information to be used
        UserInfo["id"] = Output!.GetProperty("id");
    }

    [Test]
    public async Task PosGetPublic()
    {
        HttpResponseMessage Response = await HttpService.GetAsync(
            URI + AuthSubDir + "public/",
            null! // no content body
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Evaluate Headers
        var Public = Response.Headers.GetValues("X-Public").ElementAt(0);
        Assert.That(Public, Is.EqualTo(KeyMaster.GetKeys().Public));
    }

    [Test, Order(1)]
    public async Task PostAuthLogout()
    {
        // Make request
        HttpResponseMessage Response = await HttpService.PostAsync(
            URI + AuthSubDir + "logout/",
            null, // no content body
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // No tokens needed on successful account deletion
    }

    [Test, Order(2)]
    public async Task PosPostPass()
    {
        // Make request to forgot
        var RequestContent = new Dictionary<string, JsonElement>
        {
            ["email"] = UserInfo["email"]
        };

        HttpResponseMessage Response = await HttpService.PostAsync(
            URI + AuthSubDir + "pass/forgot/",
            RequestContent,
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Make request to reset
        UserInfo["password"] = JsonSerializer.SerializeToElement(
                KeyMaster.Encrypt("TestPassword2"));
        RequestContent = new Dictionary<string, JsonElement>
        {
            ["password"] = UserInfo["password"]
        };

        using var Context = DatabaseManager.GetActiveContext();
        var UserAuth = Context.Authentications
            .Include(a => a.User)
            .First(a => a.User.Email == UserInfo["email"].GetString());

        var Headers = new Dictionary<string, string>
        {
            ["X-Reset-Token"] = Base64UrlEncoder.Encode(UserAuth.Reset)
        };

        Console.WriteLine(Headers["X-Reset-Token"]);

        Response = await HttpService.PostAsync(
            URI + AuthSubDir + "pass/reset/",
            RequestContent,
            Headers: Headers
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Make request to verify tokens are invalidated
        Response = await HttpService.PostAsync(
            URI + AuthSubDir + "verify/",
            null!,
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test, Order(3)]
    public async Task PostAuthLogin()
    {
        // Make request to /auth/login
        var RequestContent = new Dictionary<string, JsonElement>()
        {
            ["email"] = UserInfo["email"]!,
            ["password"] = UserInfo["password"]!
        };

        HttpResponseMessage Response = await HttpService.PostAsync(
            URI + AuthSubDir + "login/",
            RequestContent
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Evaluate Content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());
        var ID = Output.GetProperty("id");

        // Manually check email for confirmation that services works

        // Check passcode verification
        using var Context = DatabaseManager.GetActiveContext();

        var UserAuth = Context.Authentications
            .FirstOrDefault(a => a.User.ID == ID.GetInt32()) ??
            throw new Exception("User authentication record is missing!");

        var ProcessedToken = Authenticator.ProcessToken(UserAuth.Reset);

        ProcessedToken["payload"].TryGetValue("psc", out var PassCode);

        // Make request to /auth/mfa
        RequestContent = new Dictionary<string, JsonElement>()
        {
            ["id"] = ID,
            ["passcode"] = JsonSerializer.SerializeToElement(PassCode)!
        };

        Response = await HttpService.PostAsync(
            URI + AuthSubDir + "mfa/",
            RequestContent
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
    }

    [Test, Order(4)]
    public async Task PosPostVerify()
    {
        // Make request
        HttpResponseMessage Response = await HttpService.PostAsync(
            URI + AuthSubDir + "verify/",
            null!,
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
    }

    [Test, Order(5)]
    public async Task NegPostVerify()
    {
        // Make request
        var ValidHeader = TestRunner.GetHeaders();
        var SplitCookie = ValidHeader["Cookie"].Split("; ");

        string[] Access = new string[2];
        string Refresh = string.Empty;
        foreach (var Cookie in SplitCookie)
        {
            var WholeCookie = Cookie.Split("=");
            string CookieKey = WholeCookie[0];
            string CookieContent = WholeCookie[1];
            if ("Authorization" == CookieKey)
            {
                Console.WriteLine(CookieContent);
                Access = CookieContent.Split("%20");
            }
            else if ("X-Refresh-Token" == CookieKey) Refresh = CookieContent;
        }

        var Header = new Dictionary<string, string>
        {
            ["Cookie"] = $"Authorization={Access[0]}asdfasdf{Access[1]}; X-Refresh-Token=asdfasdf{Refresh}"
        };

        HttpResponseMessage Response = await HttpService.PostAsync(
            URI + AuthSubDir + "verify/",
            null!,
            Headers: Header
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        // No tokens to save from headers, exception occurred in authentication
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        // Make request    
        HttpResponseMessage Response = await HttpService.DeleteAsync(
            URI + UserSubDir,
            Parameter: $"{UserInfo["id"].GetInt32()!}",
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}