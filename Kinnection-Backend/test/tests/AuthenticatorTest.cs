using Kinnection;
using NUnit.Framework;
using System.Net;
using System.Security.Authentication;

namespace test;

[TestFixture, FixtureLifeCycle(LifeCycle.SingleInstance)]
public class AuthenticatorTest
{
    private KinnectionContext? Context;
    private readonly User AuthUser = new()
    {
        Created = DateTime.UtcNow,
        Fname = "AuthFname",
        Lname = "AuthLname",
        Email = "AuthEmail@mail.com",
        GoogleSO = false
    };
    private readonly string URL = TestRunner.GetURI() + "users/";

    [OneTimeSetUp]
    public void SetUp()
    {
        Context = DatabaseManager.GetActiveContext();

        // Create new user
        Context.Add(AuthUser);
        Context.SaveChanges();
    }

    [Test, Order(1)]
    public void PosProvision()
    {
        var Tokens = Authenticator.Provision(AuthUser.ID);
        TestRunner.CheckTokens(Tokens: Tokens);
        TestRunner.SaveTokens(Tokens: Tokens);
    }

    [Test, Order(2)]
    public void PosAuthenticate()
    {
        // NOTE: This method does not check the httpContext side of this method,
        //  only the core functionality. If an issue occurs with httpContext,
        //  the endpoint tests would fail but this will pass.

        var (Tokens, _) = Authenticator.Authenticate(Context!, Tokens: TestRunner.GetTokens());
        TestRunner.CheckTokens(Tokens: Tokens);
        TestRunner.SaveTokens(Tokens: Tokens);

        // Repeat to check for issues in processing
        (Tokens, _)= Authenticator.Authenticate(Context!, Tokens: Tokens);
        TestRunner.CheckTokens(Tokens: Tokens);
        TestRunner.SaveTokens(Tokens: Tokens);
    }

    [Test, Order(3)]
    public async Task NegAuthenticateHttpRequest()
    {
        var RequestContent = new Dictionary<string, string>()
        {
            ["fname"] = "Auth",
            ["lname"] = "Test",
            ["email"] = "AuthTest@mail.com"
        };

        HttpResponseMessage Response = await HttpService.PutAsync(
            URL,
            RequestContent,
            Parameter: AuthUser.ID.ToString()!
        );

        // Ensure expected status code
        Assert.That(Response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test, Order(4)]
    public void NegAuthenticate()
    {
        Authenticator.Provision(AuthUser.ID);
        try { Authenticator.Authenticate(Context!, Tokens: TestRunner.GetTokens()); }
        catch (AuthenticationException) { } // Expected Behavior
        // Any other exception occuring == test fail
    }

    [Test, Order(5)]
    public void NegProvision()
    {
        try { Authenticator.Provision(0); }
        catch (KeyNotFoundException){ } // expected behavior
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        // Remove new user
        Context!.Authentications.RemoveRange(
            Context.Authentications.Where(a => a.UserID == AuthUser.ID));
        Context.Users.Remove(AuthUser);

        Context.SaveChanges();
    }
}