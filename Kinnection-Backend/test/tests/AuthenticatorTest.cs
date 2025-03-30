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
        Assert.That(Authenticator.VerifyToken(Tokens["access"]), Is.True);
        Assert.That(Authenticator.VerifyToken(Tokens["refresh"]), Is.True);
    }

    [Test, Order(2)]
    public void PosAuthenticate()
    {
        Assert.That(Tokens, !Is.Null);
        Authenticator.Authenticate(Context!, Tokens: Tokens);
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
        Authenticator.Provision(0);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        // delete user created for testing
    }
}