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
            ""email"": ""AuthEmail@mail.com"",
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
        TestRunner.SaveTokens(Response.Headers);

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
    public async Task PostAuthLogin()
    {
        // Make request
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
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);
    }

    [Test, Order(3)]
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
        TestRunner.SaveTokens(Response.Headers);
    }

    [Test, Order(4)]
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
            ["X-Reset-Token"] = Base64UrlEncoder.Encode(UserAuth.Refresh)
        };

        Console.WriteLine(Headers["X-Reset-Token"]);

        Response = await HttpService.PostAsync(
            URI + AuthSubDir + "pass/reset/",
            RequestContent,
            Headers: Headers
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Make request to login again with new credentials
        RequestContent = new Dictionary<string, JsonElement>()
        {
            ["email"] = UserInfo["email"]!,
            ["password"] = UserInfo["password"]!
        };

        Response = await HttpService.PostAsync(
            URI + AuthSubDir + "login/",
            RequestContent
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);
    }

    // [Test, Order(3)]
    // public async Task PosGetUsers()
    // {
    //     // Make request
    //     HttpResponseMessage Response = await HttpService.GetAsync(
    //         URI + UserSubDir,
    //         Parameter: $"{UserInfo["id"].GetInt32()!}",
    //         Headers: TestRunner.GetHeaders()
    //     );

    //     // Ensure expected status code
    //     Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

    //     // Verify and save tokens
    //     TestRunner.CheckTokens(Response.Headers);
    //     TestRunner.SaveTokens(Response.Headers);

    //     // Build expected output
    //     var Expected = JsonSerializer.SerializeToElement(
    //         new Dictionary<string, JsonElement>
    //         {
    //             ["id"] = UserInfo["id"],
    //             ["fname"] = UserInfo["fname"],
    //             ["lname"] = UserInfo["lname"],
    //             ["email"] = UserInfo["email"]
    //         }
    //     );

    //     // Evaluate content
    //     var Output = JsonSerializer.Deserialize<JsonElement>(
    //         await Response.Content.ReadAsStringAsync());

    //     TestRunner.EvaluateJsonElementObject(Output, Expected);
    // }

    // [Test, Order(4)]
    // public async Task NegDeleteUsers()
    // {
    //     // Ensure unauthorized access is prevented
    //     // Make request with invalid tokens
    //     var Header = TestRunner.GetHeaders();
    //     Header["Authorization"] = Header["Authorization"] + "1";
    //     Header["X-Refresh-Token"] = Header["X-Refresh-Token"] + "1";

    //     var Response = await HttpService.DeleteAsync(
    //         URI + UserSubDir,
    //         Parameter: UserInfo["id"].GetInt32().ToString(),
    //         Headers: Header
    //     );

    //     Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    //     // No tokens to save from headers, exception occurred in authentication

    //     // Make request with empty tokens
    //     Response = await HttpService.DeleteAsync(
    //         URI + UserSubDir,
    //         Parameter: UserInfo["id"].GetInt32().ToString()
    //     );

    //     // Ensure expected status code
    //     Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    //     // No tokens to save from headers, exception occurred in authentication

    //     // Expect Not Found when user does not exist
    //     // Delete non-existent user
    //     Response = await HttpService.DeleteAsync(
    //         URI + UserSubDir,
    //         Parameter: "0",
    //         Headers: TestRunner.GetHeaders()
    //     );

    //     Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

    //     TestRunner.CheckTokens(Response.Headers);
    //     TestRunner.SaveTokens(Response.Headers);
    // }

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