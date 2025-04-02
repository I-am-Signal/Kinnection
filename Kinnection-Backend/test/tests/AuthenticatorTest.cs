using Kinnection;
using NUnit.Framework;
using System.Net;
using System.Security.Authentication;

namespace test;

[TestFixture, FixtureLifeCycle(LifeCycle.SingleInstance)]
public class AuthenticatorTest
{
    private KinnectionContext? Context;
    private int? UserID;

    [OneTimeSetUp]
    public void SetUp()
    {
        Context = DatabaseManager.GetActiveContext();
        UserID = 47; // hardcoded until user is made on demand
        // create user for testing
    }

    [Test, Order(1)]
    public void PosProvision()
    {
        var Tokens = Authenticator.Provision((int)UserID!);
        TestRunner.CheckTokens(Tokens: Tokens);
        TestRunner.SaveTokens(Tokens: Tokens);
    }

    [Test, Order(2)]
    public void PosAuthenticate()
    {
        // NOTE: This method does not check the httpContext side of this method,
        //  only the core functionality. If an issue occurs with httpContext,
        //  the endpoint tests would fail but this will pass.

        var Tokens = Authenticator.Authenticate(Context!, Tokens: TestRunner.GetTokens());
        TestRunner.CheckTokens(Tokens: Tokens);
        TestRunner.SaveTokens(Tokens: Tokens);

        // Repeat to check for issues in processing
        Tokens = Authenticator.Authenticate(Context!, Tokens: Tokens);
        TestRunner.CheckTokens(Tokens: Tokens);
        TestRunner.SaveTokens(Tokens: Tokens);
    }

    [Test, Order(3)]
    public async Task NegAuthenticateHttpContext()
    {
        Authenticator.Provision((int)UserID!);
        var RequestContent = new Dictionary<string, string>()
        {
            ["fname"] = "Auth",
            ["lname"] = "Test",
            ["email"] = "AuthTest@mail.com"
        };

        var Headers = new Dictionary<string, string>()
        {
            ["Authorization"] = "Bearer XXXXX",
            ["X-Refresh-Token"] = "XXXXX"
        };

        HttpResponseMessage Response = await HttpService.PutAsync(
            TestRunner.GetURI() + "users/",
            RequestContent,
            Parameter: $"{UserID}",
            Headers: Headers
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test, Order(4)]
    public void NegAuthenticate()
    {
        try
        {
            // After previous test, stored tokens are not the same
            Authenticator.Authenticate(Context!, Tokens: TestRunner.GetTokens());
        }
        catch (AuthenticationException a)
        {
            Assert.That(a, Is.InstanceOf<AuthenticationException>());
        }
        // Any other exception occuring == test fail
    }

    [Test, Order(5)]
    public void NegProvision()
    {
        try { Authenticator.Provision(0); }
        catch (KeyNotFoundException k) { Assert.That(k, Is.InstanceOf<KeyNotFoundException>()); }
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        // delete user created for testing
    }
}