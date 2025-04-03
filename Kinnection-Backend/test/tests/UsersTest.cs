using Kinnection;
using System.Net;
using System.Text.Json;
using NUnit.Framework;

namespace test;

[TestFixture]
public class UsersTest
{
    private readonly string URI = TestRunner.GetURI();
    private readonly string UserSubDir = "users/";
    private Dictionary<string, JsonElement> UserInfo =
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
        @"{
            ""fname"": ""PostFirst"",
            ""lname"": ""PostLast"",
            ""email"": ""PostEmail@mail.com"",
            ""password"": ""TestPassword""
        }")!;

    [OneTimeSetUp]
    public void SetUp() { }

    [Test, Order(1)]
    public async Task PosPostUsers()
    {
        // Make request
        var RequestContent = new Dictionary<string, string>()
        {
            ["fname"] = UserInfo["fname"].GetString()!,
            ["lname"] = UserInfo["lname"].GetString()!,
            ["email"] = UserInfo["email"].GetString()!,
            ["password"] = KeyMaster.Encrypt(
                UserInfo["password"].GetString()!,
                TestRunner.EncryptionKeys.Public)
        };

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

        // Verify location (expected type: int)
        Convert.ToInt32(Response.Headers.Location!.ToString());

        // Evaluate content
        var output = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            await Response.Content.ReadAsStringAsync());

        output!["id"].GetInt32();
        Assert.That(output["fname"].GetString(), Is.EqualTo(RequestContent["fname"]));
        Assert.That(output["lname"].GetString(), Is.EqualTo(RequestContent["lname"]));
        Assert.That(output["email"].GetString(), Is.EqualTo(RequestContent["email"]));

        // Save information to be used
        UserInfo["id"] = output!["id"];
    }

    [Test, Order(2)]
    public async Task PosPutUsers()
    {
        // Make request
        UserInfo["fname"] = TestRunner.ToJsonElement("PutFirst");
        UserInfo["lname"] = TestRunner.ToJsonElement("PutLast");
        UserInfo["email"] = TestRunner.ToJsonElement("PutEmail@mail.com");

        var RequestContent = new Dictionary<string, string>()
        {
            ["fname"] = UserInfo["fname"].GetString()!,
            ["lname"] = UserInfo["lname"].GetString()!,
            ["email"] = UserInfo["email"].GetString()!
        };

        HttpResponseMessage Response = await HttpService.PutAsync(
            URI + UserSubDir,
            RequestContent,
            Parameter: $"{UserInfo["id"].GetInt32()!}",
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Evaluate content
        var output = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            await Response.Content.ReadAsStringAsync());

        output!["id"].GetInt32();
        Assert.That(output["fname"].GetString(), Is.EqualTo(RequestContent["fname"]));
        Assert.That(output["lname"].GetString(), Is.EqualTo(RequestContent["lname"]));
        Assert.That(output["email"].GetString(), Is.EqualTo(RequestContent["email"]));
    }

    [Test, Order(3)]
    public async Task PosGetUsers()
    {
        // Make request
        HttpResponseMessage Response = await HttpService.GetAsync(
            URI + UserSubDir,
            Parameter: $"{UserInfo["id"].GetInt32()!}",
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Evaluate content
        var output = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            await Response.Content.ReadAsStringAsync());

        Assert.That(output!["id"].GetInt32(), Is.EqualTo(UserInfo["id"].GetInt32()));
        Assert.That(output["fname"].GetString(), Is.EqualTo(UserInfo["fname"].GetString()));
        Assert.That(output["lname"].GetString(), Is.EqualTo(UserInfo["lname"].GetString()));
        Assert.That(output["email"].GetString(), Is.EqualTo(UserInfo["email"].GetString()));
    }

    [Test, Order(4)]
    public async Task NegDeleteUsers()
    {
        // Ensure unauthorized access is prevented
        // Make request with invalid tokens
        var Header = TestRunner.GetHeaders();
        Header["Authorization"] = Header["Authorization"] + "1";
        Header["X-Refresh-Token"] = Header["X-Refresh-Token"] + "1";

        var Response = await HttpService.DeleteAsync(
            URI + UserSubDir,
            Parameter: UserInfo["id"].GetInt32().ToString(),
            Headers: Header
        );

        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        // No tokens to save from headers, exception occurred in authentication

        // Make request with empty tokens
        Response = await HttpService.DeleteAsync(
            URI + UserSubDir,
            Parameter: UserInfo["id"].GetInt32().ToString()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        // No tokens to save from headers, exception occurred in authentication

        // Expect Not Found when user does not exist
        // Delete non-existent user
        Response = await HttpService.DeleteAsync(
            URI + UserSubDir,
            Parameter: "0",
            Headers: TestRunner.GetHeaders()
        );

        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);
    }

    [Test, Order(5)]
    public async Task PosDeleteUsers()
    {
        // Make request    
        HttpResponseMessage Response = await HttpService.DeleteAsync(
            URI + UserSubDir,
            Parameter: $"{UserInfo["id"].GetInt32()!}",
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);
    }

    [OneTimeTearDown]
    public void TearDown() { }
}