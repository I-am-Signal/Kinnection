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

    private Dictionary<string, JsonElement> UserInfo = [];

    [OneTimeSetUp]
    public void Initialize()
    {
        TestRunner.Public = KeyMaster.GetKeys().Public;
        UserInfo = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            @"{
                ""fname"": ""PostFirst"",
                ""lname"": ""PostLast"",
                ""email"": ""PostEmail@mail.com"",
                ""password"": ""TestPassword""
            }")!;
    }

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
                TestRunner.Public)
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

        var Header = new Dictionary<string, string>()
        {
            ["Authorization"] = $"Bearer {TestRunner.Access}",
            ["X-Refresh-Token"] = TestRunner.Refresh
        };
    
        HttpResponseMessage Response = await HttpService.PutAsync(
            URI + UserSubDir,
            RequestContent,
            Parameter: $"{UserInfo["id"].GetInt32()!}",
            Headers: Header
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

    [OneTimeTearDown]
    public void TearDown() { }
}