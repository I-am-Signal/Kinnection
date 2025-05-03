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
        var RequestContent = new Dictionary<string, JsonElement>()
        {
            ["fname"] = UserInfo["fname"]!,
            ["lname"] = UserInfo["lname"]!,
            ["email"] = UserInfo["email"]!,
            ["password"] = JsonSerializer.SerializeToElement(KeyMaster.Encrypt(
                UserInfo["password"].GetString()!))
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

        // Build expected output
        RequestContent["id"] = JsonSerializer.SerializeToElement<int?>(null);
        var Expected = JsonSerializer.SerializeToElement(RequestContent);
        
        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());
        TestRunner.EvaluateJsonElementObject(Output, Expected);

        // Save information to be used
        UserInfo["id"] = Output!.GetProperty("id");
    }

    [Test, Order(2)]
    public async Task PosPutUsers()
    {
        // Make request
        UserInfo["fname"] = JsonSerializer.SerializeToElement("PutFirst");
        UserInfo["lname"] = JsonSerializer.SerializeToElement("PutLast");
        UserInfo["email"] = JsonSerializer.SerializeToElement("PutEmail@mail.com");

        var RequestContent = new Dictionary<string, JsonElement>()
        {
            ["fname"] = UserInfo["fname"]!,
            ["lname"] = UserInfo["lname"]!,
            ["email"] = UserInfo["email"]!
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

        // Build expected output
        RequestContent["id"] = JsonSerializer.SerializeToElement<int?>(null);
        var Expected = JsonSerializer.SerializeToElement(RequestContent);

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        TestRunner.EvaluateJsonElementObject(Output, Expected);
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

        // Build expected output
        var Expected = JsonSerializer.SerializeToElement(
            new Dictionary<string, JsonElement>{
                ["id"] = UserInfo["id"],
                ["fname"] = UserInfo["fname"],
                ["lname"] = UserInfo["lname"],
                ["email"] = UserInfo["email"]
            }
        );

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());
        
        TestRunner.EvaluateJsonElementObject(Output, Expected);
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
        // No tokens to save

        // Ensure user no longer exists
        using var Context = DatabaseManager.GetActiveContext();
        var Exists = Context.Users.FirstOrDefault(u => u.ID == UserInfo["id"].GetInt32());
        Assert.That(Exists, Is.Null);
    }

    [OneTimeTearDown]
    public void TearDown() { }
}