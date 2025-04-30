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
                UserInfo["password"].GetString()!,
                TestRunner.EncryptionKeys.Public))
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
        TestRunner.SaveTokens(Response.Headers);

        // Verify location (expected type: int)
        Convert.ToInt32(Response.Headers.Location!.ToString());

        // Evaluate content
        var output = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            await Response.Content.ReadAsStringAsync());

        output!["id"].GetInt32();
        Assert.That(output["fname"].GetString(), Is.EqualTo(RequestContent["fname"].GetString()));
        Assert.That(output["lname"].GetString(), Is.EqualTo(RequestContent["lname"].GetString()));
        Assert.That(output["email"].GetString(), Is.EqualTo(RequestContent["email"].GetString()));

        // Save information to be used
        UserInfo["id"] = output!["id"];
        TreeInfo["user_id"] = output!["id"];
    }

    [Test, Order(1)]
    public async Task PosPostTrees()
    {
        // Make request
        var RequestContent = new Dictionary<string, JsonElement>()
        {
            ["name"] = TreeInfo["name"]!
        };

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
        TestRunner.SaveTokens(Response.Headers);

        // Verify location (expected type: int)
        Convert.ToInt32(Response.Headers.Location!.ToString());

        // Evaluate content
        var output = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            await Response.Content.ReadAsStringAsync());

        foreach (var kvp in output!)
        {
            Console.WriteLine(kvp.Key + ": " + kvp.Value);
        }

        output!["id"].GetInt32();
        Assert.That(output["name"].GetString(), Is.EqualTo(RequestContent["name"].GetString()));
        Assert.That(output["member_self_id"].ValueKind, Is.EqualTo(JsonValueKind.Null));

        // Save information to be used
        TreeInfo["id"] = output!["id"];
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

        Console.WriteLine("Status Code: " + Response.StatusCode);
        Console.WriteLine("Reason Phrase: " + Response.ReasonPhrase);
        Console.WriteLine("Content: " + await Response.Content.ReadAsStringAsync());


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
        Assert.That(output["name"].GetString(), Is.EqualTo(RequestContent["name"].GetString()));
        Assert.That(output["member_self_id"].GetInt32(), Is.EqualTo(RequestContent["member_self_id"].GetInt32()));
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
        TestRunner.SaveTokens(Response.Headers);

        // Evaluate content
        var output = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            await Response.Content.ReadAsStringAsync());

        Assert.That(output!["id"].GetInt32(), Is.EqualTo(TreeInfo["id"].GetInt32()));
        Assert.That(output["name"].GetString(), Is.EqualTo(TreeInfo["name"].GetString()));
        Assert.That(output["member_self_id"].GetInt32(), Is.EqualTo(TreeInfo["member_self_id"].GetInt32()));
        foreach (JsonElement Element in output["members"].EnumerateArray())
        {
            Element.GetProperty("id").GetInt32();
            Element.GetProperty("fname").GetString();
            Element.GetProperty("mnames").GetString();
            Element.GetProperty("lname").GetString();
            Element.GetProperty("sex").GetBoolean();
            Element.GetProperty("dob").GetDateTime();
            Element.GetProperty("dod").GetDateTime();
            foreach (JsonElement SubElement in Element.GetProperty("spouses").EnumerateArray())
            {
                SubElement.GetProperty("id").GetInt32();
                SubElement.GetProperty("husband_id").GetInt32();
                SubElement.GetProperty("wife_id").GetInt32();
                SubElement.GetProperty("started").GetDateTime();
                SubElement.GetProperty("ended").GetDateTime();
            }
            foreach (JsonElement SubElement in Element.GetProperty("children").EnumerateArray())
            {
                SubElement.GetProperty("id").GetInt32();
                SubElement.GetProperty("parent_id").GetInt32();
                SubElement.GetProperty("child_id").GetInt32();
                SubElement.GetProperty("adopted").GetDateTime();
            }
        }
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
        TestRunner.SaveTokens(Response.Headers);

        // Evaluate content
        var output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        foreach (var Element in output!.GetProperty("trees").EnumerateArray())
        {
            Element.GetProperty("id").GetInt32();
            Element.GetProperty("name").GetString();
            var MemSelfID = Element.GetProperty("member_self_id");
            if (MemSelfID.ValueKind != JsonValueKind.Null)
                MemSelfID.GetInt32();
        }
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
        TestRunner.SaveTokens(Response.Headers);
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

        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);
    }
}