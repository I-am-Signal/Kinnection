using Kinnection;
using NUnit.Framework;
using System.Security.Authentication;

namespace test;

[TestFixture, FixtureLifeCycle(LifeCycle.SingleInstance)]
public class AuthenticatorTest
{
    private KinnectionContext? Context;
    private Dictionary<string, string>? Tokens;
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
        Tokens = Authenticator.Provision((int)UserID!);
        TestRunner.CheckTokens(Tokens: Tokens);
    }

    [Test, Order(2)]
    public void PosAuthenticate()
    {
        // NOTE: This method does not check the httpContext side of this method,
        //  only the core functionality. If an issue occurs with httpContext,
        //  the endpoint tests would fail but this will pass.

        Assert.That(Tokens, !Is.Null);
        Tokens = Authenticator.Authenticate(Context!, Tokens: Tokens);
        TestRunner.CheckTokens(Tokens: Tokens);

        // Repeat to check for issues in processing
        Tokens = Authenticator.Authenticate(Context!, Tokens: Tokens);
        TestRunner.CheckTokens(Tokens: Tokens);
    }

    [Test, Order(3)]
    public void NegAuthenticate()
    {
        Authenticator.Provision((int)UserID!);
        try
        {
            Authenticator.Authenticate(Context!, Tokens: Tokens);
        }
        catch (AuthenticationException a)
        {
            Assert.That(a, Is.InstanceOf<AuthenticationException>());
        }
        // Any other exception occuring == test fail
    }

    [Test, Order(4)]
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