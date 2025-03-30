using Kinnection;
using NUnit.Framework;
using System.Security.Authentication;

namespace test;

[TestFixture]
public class PassForgeTest
{
    private KinnectionContext? Context;
    private string? Password;
    private int? UserID;

    [OneTimeSetUp]
    public void SetUp()
    {
        Context = DatabaseManager.GetActiveContext();
        Password = "TestPassword";
        UserID = 47; // hardcoded until user is made on demand
        // create user for testing
    }

    [Test, Order(1)]
    public void PosHashPass()
    {
        string Hash = PassForge.HashPass(Password!);
        Assert.That(string.IsNullOrEmpty(Hash), Is.False);
        Assert.That(Hash, Is.InstanceOf<string>());
    }

    [Test, Order(2)]
    public void IsCorrect()
    {
        // Positive
        Assert.That(
            PassForge.IsPassCorrect(Password!, (int)UserID!),
            Is.True);

        // Negative
        Assert.That(
            PassForge.IsPassCorrect("asdf", (int)UserID!),
            Is.False
        );

        // Negative
        Assert.That(
            PassForge.IsPassCorrect(null!, (int)UserID!),
            Is.False
        );
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        // delete user created for testing
    }
}