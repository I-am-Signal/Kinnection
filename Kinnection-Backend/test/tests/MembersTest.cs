using Kinnection;
using System.Net;
using System.Text.Json;
using NUnit.Framework;

namespace test;

[TestFixture]
public class MembersTest
{
    private readonly KinnectionContext Context = DatabaseManager.GetActiveContext();
    private readonly string URI = TestRunner.GetURI();
    private readonly string MembersSubDir = "members/";
    private readonly string TreesSubDir = "trees/";
    private readonly string UsersSubDir = "users/";

    private Dictionary<string, JsonElement> MemberInfo =
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            JsonSerializer.Serialize(new
            {
                fname = "Mem",
                mnames = "Ber Mem",
                lname = "Ber",
                sex = false,
                dob = "2000-01-01",
                birthplace = (string?)null,
                dod = "2026-01-01",
                deathplace = (string?)null,
                death_cause = (string?)null,
                ethnicity = (string?)null,
                biography = (string?)null
            })
        )!;
    private Dictionary<string, JsonElement> TreeInfo =
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
        @"{""name"": ""TreeName""}")!;
    private Dictionary<string, JsonElement> UserInfo =
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
        @$"{{
            ""fname"": ""MemberFirst"",
            ""lname"": ""MemberLast"",
            ""email"": ""MemberEmail@mail.com"",
            ""password"": ""{Authenticator.GenerateRandomString()}""
        }}")!;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        // Get a User for testing
        // Make request
        var RequestContent = new Dictionary<string, JsonElement>()
        {
            ["fname"] = UserInfo!["fname"]!,
            ["lname"] = UserInfo["lname"]!,
            ["email"] = UserInfo["email"]!,
            ["password"] = TestRunner.ToJsonElement(KeyMaster.Encrypt(
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
        var Output = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            await Response.Content.ReadAsStringAsync());

        Output!["id"].GetInt32();
        Assert.That(Output["fname"].GetString(), Is.EqualTo(RequestContent["fname"].GetString()));
        Assert.That(Output["lname"].GetString(), Is.EqualTo(RequestContent["lname"].GetString()));
        Assert.That(Output["email"].GetString(), Is.EqualTo(RequestContent["email"].GetString()));

        // Save information to be used
        UserInfo["id"] = Output!["id"];
        TreeInfo["user_id"] = Output!["id"];

        // Get a Tree for testing
        var NewTree = new Tree
        {
            User = Context.Users.First(u => u.ID == TreeInfo["user_id"].GetInt32()),
            Created = DateTime.UtcNow,
            Name = TreeInfo["name"].GetString()!,
            SelfID = null
        };

        Context.Trees.Add(NewTree);
        Context.SaveChanges();

        TreeInfo["id"] = TestRunner.ToJsonElement(NewTree.ID);
    }

    [Test, Order(1)]
    public async Task PosPostMembers()
    {
        // Make request
        var RequestContent = new Dictionary<string, JsonElement>()
        {
            ["tree_id"] = TreeInfo["id"],
            ["fname"] = MemberInfo["fname"],
            ["mnames"] = MemberInfo["mnames"],
            ["lname"] = MemberInfo["lname"],
            ["sex"] = MemberInfo["sex"],
            ["dob"] = MemberInfo["dob"],
            ["birthplace"] = MemberInfo["birthplace"],
            ["dod"] = MemberInfo["dod"],
            ["deathplace"] = MemberInfo["deathplace"],
            ["death_cause"] = MemberInfo["death_cause"],
            ["ethnicity"] = MemberInfo["ethnicity"],
            ["biography"] = MemberInfo["biography"]
        };

        HttpResponseMessage Response = await HttpService.PostAsync(
            $"{URI}{TreesSubDir}{TreeInfo["id"].GetInt32()}/{MembersSubDir}",
            RequestContent,
            Headers: TestRunner.GetHeaders()
        );

        // Make expected JSON output
        RequestContent["id"] = TestRunner.ToJsonElement<string>(null!);
        RequestContent["spouses"] = TestRunner.ToJsonElement<List<JsonElement>>([]);
        RequestContent["children"] = TestRunner.ToJsonElement<List<JsonElement>>([]);

        var Expected = JsonSerializer.Deserialize<JsonElement>(
            JsonSerializer.Serialize(RequestContent));

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Verify location (expected type: int)
        Convert.ToInt32(Response.Headers.Location!.ToString());

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        TestRunner.EvaluateJsonElementObject(Output, Expected);

        // Save information to be used
        MemberInfo["id"] = Output!.GetProperty("id");
    }

    [Test, Order(2)]
    public async Task PosPutMembers()
    {
        // Make request
        MemberInfo["fname"] = TestRunner.ToJsonElement("PutFirst");
        MemberInfo["mnames"] = TestRunner.ToJsonElement("Put Mid");
        MemberInfo["lname"] = TestRunner.ToJsonElement("PutLast");
        MemberInfo["sex"] = TestRunner.ToJsonElement(true);
        MemberInfo["dob"] = TestRunner.ToJsonElement("2000-01-01");
        MemberInfo["birthplace"] = TestRunner.ToJsonElement("VS Code");
        MemberInfo["dod"] = TestRunner.ToJsonElement("2026-01-01");
        MemberInfo["deathplace"] = TestRunner.ToJsonElement("GitHub");
        MemberInfo["death_cause"] = TestRunner.ToJsonElement("Too many commits");
        MemberInfo["ethnicity"] = TestRunner.ToJsonElement("JSON string");
        MemberInfo["biography"] = TestRunner.ToJsonElement("PutFirst PutLast was a great JSON string.");

        var RequestContent = new Dictionary<string, JsonElement>()
        {
            ["tree_id"] = TreeInfo["id"],
            ["fname"] = MemberInfo["fname"],
            ["mnames"] = MemberInfo["mnames"],
            ["lname"] = MemberInfo["lname"],
            ["sex"] = MemberInfo["sex"],
            ["dob"] = MemberInfo["dob"],
            ["birthplace"] = MemberInfo["birthplace"],
            ["dod"] = MemberInfo["dod"],
            ["deathplace"] = MemberInfo["deathplace"],
            ["death_cause"] = MemberInfo["death_cause"],
            ["ethnicity"] = MemberInfo["ethnicity"],
            ["biography"] = MemberInfo["biography"]
        };

        HttpResponseMessage Response = await HttpService.PutAsync(
            URI + TreesSubDir + MembersSubDir + MemberInfo["id"].GetInt32(),
            RequestContent,
            Headers: TestRunner.GetHeaders()
        );

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Build expected values
        RequestContent["id"] = MemberInfo["id"];
        RequestContent["spouses"] = TestRunner.ToJsonElement<List<JsonElement>>([]);
        RequestContent["children"] = TestRunner.ToJsonElement<List<JsonElement>>([]);

        var Expected = JsonSerializer.Deserialize<JsonElement>(
            JsonSerializer.Serialize(RequestContent));

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        TestRunner.EvaluateJsonElementObject(Output, Expected);
    }

    [Test, Order(3)]
    public async Task PosGetIndividualMembers()
    {
        // Make request
        HttpResponseMessage Response = await HttpService.GetAsync(
            URI + TreesSubDir + MembersSubDir,
            Parameter: $"{MemberInfo["id"].GetInt32()!}",
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Build expected values
        var Expected = TestRunner.ToJsonElement(
            new Dictionary<string, JsonElement>()
            {
                ["id"] = MemberInfo["id"],
                ["fname"] = MemberInfo["fname"],
                ["mnames"] = MemberInfo["mnames"],
                ["lname"] = MemberInfo["lname"],
                ["sex"] = MemberInfo["sex"],
                ["dob"] = MemberInfo["dob"],
                ["dod"] = MemberInfo["dod"],
                ["spouses"] = TestRunner.ToJsonElement<List<JsonElement>>([]),
                ["children"] = TestRunner.ToJsonElement<List<JsonElement>>([]),
                ["birthplace"] = MemberInfo["birthplace"],
                ["deathplace"] = MemberInfo["deathplace"],
                ["death_Cause"] = MemberInfo["death_cause"],
                ["ethnicity"] = MemberInfo["ethnicity"],
                ["biography"] = MemberInfo["biography"],
                ["residences"] = TestRunner.ToJsonElement<List<JsonElement>>([]),
                ["emails"] = TestRunner.ToJsonElement<List<JsonElement>>([]),
                ["phones"] = TestRunner.ToJsonElement<List<JsonElement>>([]),
                ["work_History"] = TestRunner.ToJsonElement<List<JsonElement>>([]),
                ["education_History"] = TestRunner.ToJsonElement<List<JsonElement>>([]),
                ["hobbies"] = TestRunner.ToJsonElement<List<JsonElement>>([])
            });

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        TestRunner.EvaluateJsonElementObject(Output, Expected);
    }

    [Test, Order(4)]
    public async Task PosGetMembers()
    {
        // Make request
        HttpResponseMessage Response = await HttpService.GetAsync(
            URI + TreesSubDir + TreeInfo["id"].GetInt32() + "/" + MembersSubDir,
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Build expected values
        var Expected = TestRunner.ToJsonElement<List<JsonElement>>(
            [TestRunner.ToJsonElement(
            new Dictionary<string, JsonElement>()
            {
                ["id"] = MemberInfo["id"],
                ["fname"] = MemberInfo["fname"],
                ["mnames"] = MemberInfo["mnames"],
                ["lname"] = MemberInfo["lname"],
                ["sex"] = MemberInfo["sex"],
                ["dob"] = MemberInfo["dob"],
                ["dod"] = MemberInfo["dod"],
                ["spouses"] = TestRunner.ToJsonElement<List<JsonElement>>([]),
                ["children"] = TestRunner.ToJsonElement<List<JsonElement>>([]),
            })]);

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
            URI + TreesSubDir + MembersSubDir,
            Parameter: $"{MemberInfo["id"].GetInt32()!}",
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