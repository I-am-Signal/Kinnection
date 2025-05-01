using Kinnection;
using NUnit.Framework;

namespace test;

[TestFixture]
public class PassForgeTest
{
    private KinnectionContext? Context;
    private User PassUser = new()
    {
        Created = DateTime.UtcNow,
        Fname = "PassFname",
        Lname = "PassLname",
        Email = "PassEmail@mail.com",
        GoogleSO = false
    };
    private readonly string Password = "PassForge";

    [OneTimeSetUp]
    public void SetUp()
    {
        Context = DatabaseManager.GetActiveContext();
        Context.Add(PassUser);
        Context.SaveChanges();
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
        // Create new password
        string Hash = PassForge.HashPass(Password!);
        Context!.Add(new Password
        {
            Created = DateTime.UtcNow,
            User = PassUser,
            PassString = Hash
        });
        Context.SaveChanges();

        Console.WriteLine(PassUser.ID);

        // Positive
        Assert.That(
            PassForge.IsPassCorrect(Password, PassUser.ID),
            Is.True);

        // Negative
        Assert.That(
            PassForge.IsPassCorrect("asdf", PassUser.ID),
            Is.False
        );

        // Negative
        Assert.That(
            PassForge.IsPassCorrect(null!, PassUser.ID),
            Is.False
        );
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        // Remove new user
        Context!.Passwords.RemoveRange(
            Context.Passwords.Where(p => p.User.ID == PassUser.ID));
        Context.Users.Remove(PassUser);

        Context.SaveChanges();
    }
}