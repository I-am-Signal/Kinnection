using System.Net;
using Newtonsoft.Json;

namespace test;

[TestClass]
public class UserTest
{
    private string URI = string.Empty;

    [TestInitialize]
    public void Initialize()
    {
        URI = TestRunner.GetURI() + "users";
    }

    [TestMethod]
    public async Task PostUser()
    {
        Dictionary<string, string> RequestContent = new Dictionary<string, string>()
        {
            ["fname"] = "TestFirst",
            ["lname"] = "TestLast",
            ["email"] = "TestEmail@mail.com",
            ["password"] = "TestPassword"
        };

        // Assert.AreEqual("http://localhost:8080/users", URI);

        HttpResponseMessage Response = await HttpService.PostAsync(
            URI,
            RequestContent
        );

        // Ensure expected status code
        Assert.AreEqual(HttpStatusCode.Created, Response.StatusCode);

        // Evaluate headers
        // string Access = Response.Headers.GetValues("Authorization").ElementAt(0).Split(" ")[1];
        // string Refresh = Response.Headers.GetValues("X-Refresh-Token").ElementAt(0);

        // Assert.IsInstanceOfType<string>(Access);
        // Assert.IsInstanceOfType<string>(Refresh);

        // TestRunner.Access = Access;
        // TestRunner.Refresh = Refresh;

        // Evaluate content
        Dictionary<string, string> ResponseContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(
            await Response.Content.ReadAsStringAsync())!;
        string ID = ResponseContent["id"];

        try
        {
            Convert.ToInt32(ID);
        }
        finally { }
    }

    [TestCleanup]
    public void CleanUp() { }
}