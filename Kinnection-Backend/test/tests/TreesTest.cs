using Kinnection;
using System.Net;
using System.Text.Json;
using NUnit.Framework;

namespace test;

[TestFixture]
public class TreesTest
{
    private readonly string URI = TestRunner.GetURI();
    private readonly string TreesSubDir = "trees/";
    private readonly string UsersSubDir = "users/";
    private Dictionary<string, JsonElement> TreeInfo =
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
        @"{""name"": ""TreeName""}")!;
    private Dictionary<string, JsonElement> UserInfo =
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
        @"{
            ""fname"": ""TreeFirst"",
            ""lname"": ""TreeLast"",
            ""email"": ""TreeEmail@mail.com"",
            ""password"": ""TreePassword""
        }")!;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        // Get a user for testing
        // Make request
        var RequestContent = new Dictionary<string, JsonElement>()
        {
            ["fname"] = UserInfo!["fname"]!,
            ["lname"] = UserInfo["lname"]!,
            ["email"] = UserInfo["email"]!,
            ["password"] = JsonSerializer.SerializeToElement(KeyMaster.Encrypt(
                UserInfo["password"].GetString()!))
        };

        HttpResponseMessage Response = await HttpService.PostAsync(
            URI + UsersSubDir,
            RequestContent
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);

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
        TreeInfo["user_id"] = Output!.GetProperty("id");
    }

    [Test, Order(1)]
    public async Task PosPostTrees()
    {
        // Make request
        var RequestContent = new Dictionary<string, JsonElement>()
        { ["name"] = TreeInfo["name"]! };

        HttpResponseMessage Response = await HttpService.PostAsync(
            URI + TreesSubDir,
            RequestContent,
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);

        // Verify location (expected type: int)
        Convert.ToInt32(Response.Headers.Location!.ToString());

        // Build expected output
        RequestContent["member_self_id"] = JsonSerializer.SerializeToElement<int?>(null);
        RequestContent["id"] = RequestContent["member_self_id"];
        var Expected = JsonSerializer.SerializeToElement(RequestContent);

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());
        TestRunner.EvaluateJsonElementObject(Output, Expected);

        // Save information to be used
        TreeInfo["id"] = Output!.GetProperty("id");
    }

    [Test, Order(2)]
    public async Task PosPutTrees()
    {
        // Make request
        TreeInfo["name"] = JsonSerializer.SerializeToElement("TreePut");
        TreeInfo["member_self_id"] = JsonSerializer.SerializeToElement(0);

        var RequestContent = new Dictionary<string, JsonElement>()
        {
            ["name"] = TreeInfo["name"],
            ["member_self_id"] = TreeInfo["member_self_id"]
        };

        HttpResponseMessage Response = await HttpService.PutAsync(
            URI + TreesSubDir,
            RequestContent,
            Parameter: $"{TreeInfo["id"].GetInt32()}",
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);

        // Build expected output
        RequestContent["id"] = JsonSerializer.SerializeToElement<int?>(null);
        var Expected = JsonSerializer.SerializeToElement(RequestContent);

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());
        TestRunner.EvaluateJsonElementObject(Output, Expected);
    }

    [Test, Order(3)]
    public async Task PosGetIndividualTrees()
    {
        // Make request
        HttpResponseMessage Response = await HttpService.GetAsync(
            URI + TreesSubDir,
            Parameter: $"{TreeInfo["id"].GetInt32()!}",
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);

        // Build expected output
        var Expected = JsonSerializer.SerializeToElement(
            new
            {
                id = TreeInfo["id"],
                name = TreeInfo["name"],
                member_self_id = TreeInfo["member_self_id"],
                members = JsonSerializer.SerializeToElement<List<GetTreesMembersResponse>>([])
            }
        );

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());
        TestRunner.EvaluateJsonElementObject(Output, Expected);
    }

    [Test, Order(4)]
    public async Task PosGetTrees()
    {
        // Make request
        HttpResponseMessage Response = await HttpService.GetAsync(
            URI + TreesSubDir,
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);

        // Build expected output
        var Expected = JsonSerializer.SerializeToElement(new
        {
            trees = new[] {
                new {
                    id = TreeInfo["id"],
                    name = TreeInfo["name"],
                    member_self_id = TreeInfo["member_self_id"],
                }
            }
        });

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());
        TestRunner.EvaluateJsonElementObject(Output, Expected);
    }

    [Test, Order(5)]
    public async Task PosDeleteTrees()
    {
        // Make request    
        HttpResponseMessage Response = await HttpService.DeleteAsync(
            URI + TreesSubDir,
            Parameter: $"{TreeInfo["id"].GetInt32()!}",
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
    }

    [Test, Order(6)]
    public async Task PosGetIndividualTrees2()
    {
        // Make request
        HttpResponseMessage Response = await HttpService.GetAsync(
            URI + TreesSubDir,
            Parameter: $"{TreeInfo["id"].GetInt32()!}",
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        // Remove user created for testing
        // Make request    
        HttpResponseMessage Response = await HttpService.DeleteAsync(
            URI + UsersSubDir,
            Parameter: $"{UserInfo["id"].GetInt32()}",
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // No tokens to save on successful account deletion
    }
}