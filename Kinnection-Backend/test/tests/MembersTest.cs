using Kinnection;
using System.Net;
using System.Text.Json;
using NUnit.Framework;

namespace test;

public class LowerCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        return name.ToLower();
    }
}

[TestFixture]
public class MembersTest
{
    JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = new LowerCaseNamingPolicy(),
        WriteIndented = true
    };
    private readonly KinnectionContext Context = DatabaseManager.GetActiveContext();
    private readonly string URI = TestRunner.GetURI();
    private readonly string MembersSubDir = "members/";
    private readonly string TreesSubDir = "trees/";
    private readonly string UsersSubDir = "users/";
    private Dictionary<string, JsonElement> MemberInfo =
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            JsonSerializer.Serialize(new
            {
                id = (int?)null,
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
    private List<PutChildrenRequest> MutableChildren = [];
    private List<PutEducationRequest> MutableEducations = [];
    private List<PutEmailsRequest> MutableEmails = [];
    private List<PutHobbiesRequest> MutableHobbies = [];
    private List<PutPhonesRequest> MutablePhones = [];
    private List<PutResidencesRequest> MutableResidences = [];
    private List<PutSpousesRequest> MutableSpouses = [];
    private List<PutWorkRequest> MutableWorks = [];
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

    private int MemberID2 = 0;
    private int MemberID3 = 0;
    private string PutURL = "";

    private Dictionary<string, JsonElement> BuildRequestContent()
    {
        return new Dictionary<string, JsonElement>()
        {
            ["id"] = MemberInfo["id"],
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
            ["biography"] = MemberInfo["biography"],
            ["children"] = JsonSerializer.SerializeToElement(MutableChildren, Options),
            ["education_history"] = JsonSerializer.SerializeToElement(MutableEducations, Options),
            ["emails"] = JsonSerializer.SerializeToElement(MutableEmails, Options),
            ["hobbies"] = JsonSerializer.SerializeToElement(MutableHobbies, Options),
            ["phones"] = JsonSerializer.SerializeToElement(MutablePhones, Options),
            ["residences"] = JsonSerializer.SerializeToElement(MutableResidences, Options),
            ["spouses"] = JsonSerializer.SerializeToElement(MutableSpouses, Options),
            ["work_history"] = JsonSerializer.SerializeToElement(MutableWorks, Options)
        };
    }

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

        TreeInfo["id"] = JsonSerializer.SerializeToElement(NewTree.ID);
    }

    [Test, Order(1)]
    public async Task PosPostMembers()
    {
        // Make request
        var RequestContent = BuildRequestContent();

        HttpResponseMessage Response = await HttpService.PostAsync(
            $"{URI}{TreesSubDir}{TreeInfo["id"].GetInt32()}/{MembersSubDir}",
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

        // Make expected JSON output
        RequestContent["id"] = JsonSerializer.SerializeToElement<string>(null!);
        var Expected = JsonSerializer.SerializeToElement(RequestContent);

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        TestRunner.EvaluateJsonElementObject(Output, Expected);

        // Save information to be used
        MemberInfo["id"] = Output!.GetProperty("id");
    }

    [Test, Order(2)]
    public async Task PosPutMembers1()
    {
        // -----------------------------------------------------------------
        // Setup: Make two additional test members and define PutURL
        // -----------------------------------------------------------------
 
        // Designate endpoint to hit
        PutURL = URI + TreesSubDir + TreeInfo["id"].GetInt32() + '/'
            + MembersSubDir + MemberInfo["id"].GetInt32();

        // Make Test Member 2
        var RequestContent = BuildRequestContent();

        HttpResponseMessage Response = await HttpService.PostAsync(
            $"{URI}{TreesSubDir}{TreeInfo["id"].GetInt32()}/{MembersSubDir}",
            RequestContent,
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Get ID
        MemberID2 = Convert.ToInt32(Response.Headers.Location!.ToString());


        // Make Test Member 3
        RequestContent["fname"] = JsonSerializer.SerializeToElement("First3");
        RequestContent["lname"] = JsonSerializer.SerializeToElement("Last3");
        RequestContent["sex"] = JsonSerializer.SerializeToElement(true);

        Response = await HttpService.PostAsync(
            $"{URI}{TreesSubDir}{TreeInfo["id"].GetInt32()}/{MembersSubDir}",
            RequestContent,
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Get ID
        MemberID3 = Convert.ToInt32(Response.Headers.Location!.ToString());

        // -----------------------------------------------------------------
        // Test 1: Modify membr attrs without any rels
        // -----------------------------------------------------------------

        // Make request
        MemberInfo["fname"] = JsonSerializer.SerializeToElement("PutFirst");
        MemberInfo["mnames"] = JsonSerializer.SerializeToElement("Put Mid");
        MemberInfo["lname"] = JsonSerializer.SerializeToElement("PutLast");
        MemberInfo["sex"] = JsonSerializer.SerializeToElement(true);
        MemberInfo["dob"] = JsonSerializer.SerializeToElement("2000-01-01");
        MemberInfo["birthplace"] = JsonSerializer.SerializeToElement("VS Code");
        MemberInfo["dod"] = JsonSerializer.SerializeToElement("2026-01-01");
        MemberInfo["deathplace"] = JsonSerializer.SerializeToElement("GitHub");
        MemberInfo["death_cause"] = JsonSerializer.SerializeToElement("Too many commits");
        MemberInfo["ethnicity"] = JsonSerializer.SerializeToElement("JSON string");
        MemberInfo["biography"] = JsonSerializer.SerializeToElement("PutFirst PutLast was a great JSON string.");

        RequestContent = BuildRequestContent();

        Response = await HttpService.PutAsync(
            PutURL,
            RequestContent,
            Headers: TestRunner.GetHeaders()
        );

        Console.WriteLine(PutURL);

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Build expected values
        RequestContent["id"] = MemberInfo["id"];
        var Expected = JsonSerializer.Deserialize<JsonElement>(
            JsonSerializer.Serialize(RequestContent));

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        TestRunner.EvaluateJsonElementObject(Output, Expected);
    }

    [Test, Order(3)]
    public async Task PosPutMembers2()
    {
        // -----------------------------------------------------------------
        // Test 2: Nullable and non-null member attrs, add rel
        // -----------------------------------------------------------------

        // Make request
        MemberInfo["fname"] = JsonSerializer.SerializeToElement("PutFirst");
        MemberInfo["mnames"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["lname"] = JsonSerializer.SerializeToElement("PutLast");
        MemberInfo["sex"] = JsonSerializer.SerializeToElement(true);
        MemberInfo["dob"] = JsonSerializer.SerializeToElement((DateOnly?)null);
        MemberInfo["birthplace"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["dod"] = JsonSerializer.SerializeToElement((DateOnly?)null);
        MemberInfo["deathplace"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["death_cause"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["ethnicity"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["biography"] = JsonSerializer.SerializeToElement((string)null!);
        MutableChildren.Add(new PutChildrenRequest
        {
            Id = null,
            Parent_id = MemberID2,
            Child_id = MemberInfo["id"].GetInt32(),
            Adopted = null
        });
        MutableEducations.Add(new PutEducationRequest
        {
            Id = null,
            Started = null,
            Ended = null,
            Title = "title1",
            Organization = null!,
            Description = null!
        });
        MutableEmails.Add(new PutEmailsRequest
        {
            Id = null,
            Email = "test1@mail.com",
            Primary = true
        });
        MutableHobbies.Add(new PutHobbiesRequest
        {
            Id = null,
            Started = null,
            Ended = null,
            Title = "title1",
            Organization = null!,
            Description = null!
        });
        MutablePhones.Add(new PutPhonesRequest
        {
            Id = null,
            Phone_number = "1234567890",
            Primary = true
        });
        MutableResidences.Add(new PutResidencesRequest
        {
            Id = null,
            Addr_line_1 = "123 Test Street",
            Addr_line_2 = null,
            City = "Testville",
            State = null,
            Country = "Testistan",
            Started = null,
            Ended = null
        });
        MutableSpouses.Add(new PutSpousesRequest
        {
            Id = null,
            Husband_id = MemberInfo["id"].GetInt32(),
            Wife_id = MemberID3,
            Started = null,
            Ended = null
        });
        MutableWorks.Add(new PutWorkRequest
        {
            Id = null,
            Started = null,
            Ended = null,
            Title = "title1",
            Organization = null!,
            Description = null!
        });

        var RequestContent = BuildRequestContent();

        var Response = await HttpService.PutAsync(
            PutURL,
            RequestContent,
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Build expected values
        RequestContent["id"] = MemberInfo["id"];
        var Expected = JsonSerializer.Deserialize<JsonElement>(
            JsonSerializer.Serialize(RequestContent));

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        TestRunner.EvaluateJsonElementObject(Output, Expected);

        // Save Ids where for rels
        MutableChildren[0].Id = Output.GetProperty("children")[0].GetProperty("id").GetInt32();
        MutableEducations[0].Id = Output.GetProperty("education_history")[0].GetProperty("id").GetInt32();
        MutableEmails[0].Id = Output.GetProperty("emails")[0].GetProperty("id").GetInt32();
        MutableHobbies[0].Id = Output.GetProperty("hobbies")[0].GetProperty("id").GetInt32();
        MutablePhones[0].Id = Output.GetProperty("phones")[0].GetProperty("id").GetInt32();
        MutableResidences[0].Id = Output.GetProperty("residences")[0].GetProperty("id").GetInt32();
        MutableSpouses[0].Id = Output.GetProperty("spouses")[0].GetProperty("id").GetInt32();
        MutableWorks[0].Id = Output.GetProperty("work_history")[0].GetProperty("id").GetInt32();
    }

    [Test, Order(4)]
    public async Task PosPutMember3()
    {

        // -----------------------------------------------------------------
        // Test 3: Same Member attrs, add rel and modify rel
        // -----------------------------------------------------------------

        // Make request
        MemberInfo["fname"] = JsonSerializer.SerializeToElement("PutFirst");
        MemberInfo["mnames"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["lname"] = JsonSerializer.SerializeToElement("PutLast");
        MemberInfo["sex"] = JsonSerializer.SerializeToElement(true);
        MemberInfo["dob"] = JsonSerializer.SerializeToElement((DateOnly?)null);
        MemberInfo["birthplace"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["dod"] = JsonSerializer.SerializeToElement((DateOnly?)null);
        MemberInfo["deathplace"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["death_cause"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["ethnicity"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["biography"] = JsonSerializer.SerializeToElement((string)null!);

        // Modify existing rels
        MutableChildren[0] = new PutChildrenRequest
        {
            Id = MutableChildren[0].Id,
            Parent_id = MemberInfo["id"].GetInt32(),
            Child_id = MemberID2,
            Adopted = DateOnly.Parse("2000-01-01")
        };
        MutableEducations[0] = new PutEducationRequest
        {
            Id = MutableEducations[0].Id,
            Started = DateOnly.Parse("2000-01-01"),
            Ended = DateOnly.Parse("2000-01-02"),
            Title = "title2",
            Organization = "organization1",
            Description = "description1"
        };
        MutableEmails[0] = new PutEmailsRequest
        {
            Id = MutableEmails[0].Id,
            Email = "test2@mail.com",
            Primary = true
        };
        MutableHobbies[0] = new PutHobbiesRequest
        {
            Id = MutableHobbies[0].Id,
            Started = DateOnly.Parse("2000-01-01"),
            Ended = DateOnly.Parse("2000-01-02"),
            Title = "title2",
            Organization = "organization1",
            Description = "description1"
        };
        MutablePhones[0] = new PutPhonesRequest
        {
            Id = MutablePhones[0].Id,
            Phone_number = "0987654321",
            Primary = false
        };
        MutableResidences[0] = new PutResidencesRequest
        {
            Id = MutableResidences[0].Id,
            Addr_line_1 = "456 Test Avenue",
            Addr_line_2 = "Apartment 123",
            City = "Testspring",
            State = "Testia",
            Country = "Testienia",
            Started = DateOnly.Parse("2000-01-01"),
            Ended = DateOnly.Parse("2000-01-02")
        };
        MutableSpouses[0] = new PutSpousesRequest
        {
            Id = MutableSpouses[0].Id,
            Wife_id = MemberInfo["id"].GetInt32(),
            Husband_id = MemberID3,
            Started = DateOnly.Parse("2000-01-01"),
            Ended = null
        };
        MutableWorks[0] = new PutWorkRequest
        {
            Id = MutableWorks[0].Id,
            Started = DateOnly.Parse("2000-01-01"),
            Ended = null,
            Title = "title2",
            Organization = "organization1",
            Description = "description1"
        };

        // Add new rels
        MutableChildren.Add(new PutChildrenRequest
        {
            Id = null,
            Parent_id = MemberID2,
            Child_id = MemberInfo["id"].GetInt32(),
            Adopted = null
        });
        MutableEducations.Add(new PutEducationRequest
        {
            Id = null,
            Started = null,
            Ended = null,
            Title = "title1",
            Organization = null!,
            Description = null!
        });
        MutableEmails.Add(new PutEmailsRequest
        {
            Id = null,
            Email = "test1@mail.com",
            Primary = true
        });
        MutableHobbies.Add(new PutHobbiesRequest
        {
            Id = null,
            Started = null,
            Ended = null,
            Title = "title1",
            Organization = null!,
            Description = null!
        });
        MutablePhones.Add(new PutPhonesRequest
        {
            Id = null,
            Phone_number = "1234567890",
            Primary = true
        });
        MutableResidences.Add(new PutResidencesRequest
        {
            Id = null,
            Addr_line_1 = "123 Test Street",
            Addr_line_2 = null,
            City = "Testville",
            State = null,
            Country = "Testistan",
            Started = null,
            Ended = null
        });
        MutableSpouses.Add(new PutSpousesRequest
        {
            Id = null,
            Husband_id = MemberInfo["id"].GetInt32(),
            Wife_id = MemberID3,
            Started = null,
            Ended = null
        });
        MutableWorks.Add(new PutWorkRequest
        {
            Id = null,
            Started = null,
            Ended = null,
            Title = "title1",
            Organization = null!,
            Description = null!
        });

        var RequestContent = BuildRequestContent();

        var Response = await HttpService.PutAsync(
            PutURL,
            RequestContent,
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Build expected values
        RequestContent["id"] = MemberInfo["id"];
        var Expected = JsonSerializer.Deserialize<JsonElement>(
            JsonSerializer.Serialize(RequestContent));

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        TestRunner.EvaluateJsonElementObject(Output, Expected);

        // Save Ids for rels
        MutableChildren[1].Id = Output.GetProperty("children")[1].GetProperty("id").GetInt32();
        MutableEducations[1].Id = Output.GetProperty("education_history")[1].GetProperty("id").GetInt32();
        MutableEmails[1].Id = Output.GetProperty("emails")[1].GetProperty("id").GetInt32();
        MutableHobbies[1].Id = Output.GetProperty("hobbies")[1].GetProperty("id").GetInt32();
        MutablePhones[1].Id = Output.GetProperty("phones")[1].GetProperty("id").GetInt32();
        MutableResidences[1].Id = Output.GetProperty("residences")[1].GetProperty("id").GetInt32();
        MutableSpouses[1].Id = Output.GetProperty("spouses")[1].GetProperty("id").GetInt32();
        MutableWorks[1].Id = Output.GetProperty("work_history")[1].GetProperty("id").GetInt32();
    }

    [Test, Order(5)]
    public async Task PosPutMembers4()
    {
        // -----------------------------------------------------------------
        // Test 4: Same Member attrs, add rel and remove rel
        // -----------------------------------------------------------------

        // Make request
        MemberInfo["fname"] = JsonSerializer.SerializeToElement("PutFirst");
        MemberInfo["mnames"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["lname"] = JsonSerializer.SerializeToElement("PutLast");
        MemberInfo["sex"] = JsonSerializer.SerializeToElement(true);
        MemberInfo["dob"] = JsonSerializer.SerializeToElement((DateOnly?)null);
        MemberInfo["birthplace"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["dod"] = JsonSerializer.SerializeToElement((DateOnly?)null);
        MemberInfo["deathplace"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["death_cause"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["ethnicity"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["biography"] = JsonSerializer.SerializeToElement((string)null!);

        MutableChildren.RemoveAt(0);
        MutableEducations.RemoveAt(0);
        MutableEmails.RemoveAt(0);
        MutableHobbies.RemoveAt(0);
        MutablePhones.RemoveAt(0);
        MutableResidences.RemoveAt(0);
        MutableSpouses.RemoveAt(0);
        MutableWorks.RemoveAt(0);

        // Add new rels
        MutableChildren.Add(new PutChildrenRequest
        {
            Id = null,
            Parent_id = MemberID2,
            Child_id = MemberInfo["id"].GetInt32(),
            Adopted = null
        });
        MutableEducations.Add(new PutEducationRequest
        {
            Id = null,
            Started = null,
            Ended = null,
            Title = "title1",
            Organization = null!,
            Description = null!
        });
        MutableEmails.Add(new PutEmailsRequest
        {
            Id = null,
            Email = "test1@mail.com",
            Primary = true
        });
        MutableHobbies.Add(new PutHobbiesRequest
        {
            Id = null,
            Started = null,
            Ended = null,
            Title = "title1",
            Organization = null!,
            Description = null!
        });
        MutablePhones.Add(new PutPhonesRequest
        {
            Id = null,
            Phone_number = "1234567890",
            Primary = true
        });
        MutableResidences.Add(new PutResidencesRequest
        {
            Id = null,
            Addr_line_1 = "123 Test Street",
            Addr_line_2 = null,
            City = "Testville",
            State = null,
            Country = "Testistan",
            Started = null,
            Ended = null
        });
        MutableSpouses.Add(new PutSpousesRequest
        {
            Id = null,
            Husband_id = MemberInfo["id"].GetInt32(),
            Wife_id = MemberID3,
            Started = null,
            Ended = null
        });
        MutableWorks.Add(new PutWorkRequest
        {
            Id = null,
            Started = null,
            Ended = null,
            Title = "title1",
            Organization = null!,
            Description = null!
        });

        var RequestContent = BuildRequestContent();

        var Response = await HttpService.PutAsync(
            PutURL,
            RequestContent,
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Build expected values
        RequestContent["id"] = MemberInfo["id"];
        var Expected = JsonSerializer.Deserialize<JsonElement>(
            JsonSerializer.Serialize(RequestContent));

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        TestRunner.EvaluateJsonElementObject(Output, Expected);

        // Save Ids for rels
        MutableChildren[1].Id = Output.GetProperty("children")[1].GetProperty("id").GetInt32();
        MutableEducations[1].Id = Output.GetProperty("education_history")[1].GetProperty("id").GetInt32();
        MutableEmails[1].Id = Output.GetProperty("emails")[1].GetProperty("id").GetInt32();
        MutableHobbies[1].Id = Output.GetProperty("hobbies")[1].GetProperty("id").GetInt32();
        MutablePhones[1].Id = Output.GetProperty("phones")[1].GetProperty("id").GetInt32();
        MutableResidences[1].Id = Output.GetProperty("residences")[1].GetProperty("id").GetInt32();
        MutableSpouses[1].Id = Output.GetProperty("spouses")[1].GetProperty("id").GetInt32();
        MutableWorks[1].Id = Output.GetProperty("work_history")[1].GetProperty("id").GetInt32();
    }

    [Test, Order(6)]
    public async Task PosPutMembers5()
    {
        // -----------------------------------------------------------------
        // Test 5: Same Member attrs, modify rel and remove rel
        // -----------------------------------------------------------------

        // Make request
        MemberInfo["fname"] = JsonSerializer.SerializeToElement("PutFirst");
        MemberInfo["mnames"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["lname"] = JsonSerializer.SerializeToElement("PutLast");
        MemberInfo["sex"] = JsonSerializer.SerializeToElement(true);
        MemberInfo["dob"] = JsonSerializer.SerializeToElement((DateOnly?)null);
        MemberInfo["birthplace"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["dod"] = JsonSerializer.SerializeToElement((DateOnly?)null);
        MemberInfo["deathplace"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["death_cause"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["ethnicity"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["biography"] = JsonSerializer.SerializeToElement((string)null!);

        // Remove some existing rels
        MutableChildren.RemoveAt(0);
        MutableEducations.RemoveAt(0);
        MutableEmails.RemoveAt(0);
        MutableHobbies.RemoveAt(0);
        MutablePhones.RemoveAt(0);
        MutableResidences.RemoveAt(0);
        MutableSpouses.RemoveAt(0);
        MutableWorks.RemoveAt(0);

        // Modify existing rels
        MutableChildren[0] = new PutChildrenRequest
        {
            Id = MutableChildren[0].Id,
            Parent_id = MemberInfo["id"].GetInt32(),
            Child_id = MemberID2,
            Adopted = DateOnly.Parse("2000-01-01")
        };
        MutableEducations[0] = new PutEducationRequest
        {
            Id = MutableEducations[0].Id,
            Started = DateOnly.Parse("2000-01-01"),
            Ended = DateOnly.Parse("2000-01-02"),
            Title = "title2",
            Organization = "organization1",
            Description = "description1"
        };
        MutableEmails[0] = new PutEmailsRequest
        {
            Id = MutableEmails[0].Id,
            Email = "test2@mail.com",
            Primary = true
        };
        MutableHobbies[0] = new PutHobbiesRequest
        {
            Id = MutableHobbies[0].Id,
            Started = DateOnly.Parse("2000-01-01"),
            Ended = DateOnly.Parse("2000-01-02"),
            Title = "title2",
            Organization = "organization1",
            Description = "description1"
        };
        MutablePhones[0] = new PutPhonesRequest
        {
            Id = MutablePhones[0].Id,
            Phone_number = "0987654321",
            Primary = false
        };
        MutableResidences[0] = new PutResidencesRequest
        {
            Id = MutableResidences[0].Id,
            Addr_line_1 = "456 Test Avenue",
            Addr_line_2 = "Apartment 123",
            City = "Testspring",
            State = "Testia",
            Country = "Testienia",
            Started = DateOnly.Parse("2000-01-01"),
            Ended = DateOnly.Parse("2000-01-02")
        };
        MutableSpouses[0] = new PutSpousesRequest
        {
            Id = MutableSpouses[0].Id,
            Wife_id = MemberInfo["id"].GetInt32(),
            Husband_id = MemberID3,
            Started = DateOnly.Parse("2000-01-01"),
            Ended = null
        };
        MutableWorks[0] = new PutWorkRequest
        {
            Id = MutableWorks[0].Id,
            Started = DateOnly.Parse("2000-01-01"),
            Ended = null,
            Title = "title2",
            Organization = "organization1",
            Description = "description1"
        };

        var RequestContent = BuildRequestContent();

        var Response = await HttpService.PutAsync(
            PutURL,
            RequestContent,
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Build expected values
        RequestContent["id"] = MemberInfo["id"];
        var Expected = JsonSerializer.Deserialize<JsonElement>(
            JsonSerializer.Serialize(RequestContent));

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        TestRunner.EvaluateJsonElementObject(Output, Expected);

        // Save Ids for rels
        MutableChildren[0].Id = Output.GetProperty("children")[0].GetProperty("id").GetInt32();
        MutableEducations[0].Id = Output.GetProperty("education_history")[0].GetProperty("id").GetInt32();
        MutableEmails[0].Id = Output.GetProperty("emails")[0].GetProperty("id").GetInt32();
        MutableHobbies[0].Id = Output.GetProperty("hobbies")[0].GetProperty("id").GetInt32();
        MutablePhones[0].Id = Output.GetProperty("phones")[0].GetProperty("id").GetInt32();
        MutableResidences[0].Id = Output.GetProperty("residences")[0].GetProperty("id").GetInt32();
        MutableSpouses[0].Id = Output.GetProperty("spouses")[0].GetProperty("id").GetInt32();
        MutableWorks[0].Id = Output.GetProperty("work_history")[0].GetProperty("id").GetInt32();
    }

    [Test, Order(7)]
    public async Task PosPutMembers6()
    {
        // -----------------------------------------------------------------
        // Test 6: Same Member attrs, remove rel
        // -----------------------------------------------------------------

        // Make request
        MemberInfo["fname"] = JsonSerializer.SerializeToElement("PutFirst");
        MemberInfo["mnames"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["lname"] = JsonSerializer.SerializeToElement("PutLast");
        MemberInfo["sex"] = JsonSerializer.SerializeToElement(true);
        MemberInfo["dob"] = JsonSerializer.SerializeToElement((DateOnly?)null);
        MemberInfo["birthplace"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["dod"] = JsonSerializer.SerializeToElement((DateOnly?)null);
        MemberInfo["deathplace"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["death_cause"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["ethnicity"] = JsonSerializer.SerializeToElement((string)null!);
        MemberInfo["biography"] = JsonSerializer.SerializeToElement((string)null!);

        // Remove existing rels
        MutableChildren.RemoveAt(0);
        MutableEducations.RemoveAt(0);
        MutableEmails.RemoveAt(0);
        MutableHobbies.RemoveAt(0);
        MutablePhones.RemoveAt(0);
        MutableResidences.RemoveAt(0);
        MutableSpouses.RemoveAt(0);
        MutableWorks.RemoveAt(0);

        var RequestContent = BuildRequestContent();

        var Response = await HttpService.PutAsync(
            PutURL,
            RequestContent,
            Headers: TestRunner.GetHeaders()
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Evaluate Headers
        // Verify and save tokens
        TestRunner.CheckTokens(Response.Headers);
        TestRunner.SaveTokens(Response.Headers);

        // Build expected values
        RequestContent["id"] = MemberInfo["id"];
        var Expected = JsonSerializer.Deserialize<JsonElement>(
            JsonSerializer.Serialize(RequestContent));

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        TestRunner.EvaluateJsonElementObject(Output, Expected);
    }

    [Test, Order(8)]
    public async Task PosGetMembers()
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
        var Expected = JsonSerializer.SerializeToElement(
            new Dictionary<string, JsonElement>()
            {
                ["id"] = MemberInfo["id"],
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
                ["biography"] = MemberInfo["biography"],
                ["children"] = JsonSerializer.SerializeToElement(MutableChildren, Options),
                ["education_history"] = JsonSerializer.SerializeToElement(MutableEducations, Options),
                ["emails"] = JsonSerializer.SerializeToElement(MutableEmails, Options),
                ["hobbies"] = JsonSerializer.SerializeToElement(MutableHobbies, Options),
                ["phones"] = JsonSerializer.SerializeToElement(MutablePhones, Options),
                ["residences"] = JsonSerializer.SerializeToElement(MutableResidences, Options),
                ["spouses"] = JsonSerializer.SerializeToElement(MutableSpouses, Options),
                ["work_history"] = JsonSerializer.SerializeToElement(MutableWorks, Options)
            });

        // Evaluate content
        var Output = JsonSerializer.Deserialize<JsonElement>(
            await Response.Content.ReadAsStringAsync());

        TestRunner.EvaluateJsonElementObject(Output, Expected);
    }

    [Test, Order(9)]
    public async Task PosDeleteMembers()
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