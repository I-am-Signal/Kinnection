using System.Security.Cryptography;
using System.Text;

namespace Kinnection;

/// <summary>
/// A record object containing strings representing Public and Private keys 
/// for an RSA 2048 bit encryption algorithm.
/// </summary>
/// <param name="Public"></param>
/// <param name="Private"></param>
public record Keys
(
    string Public,
    string Private
);

public static class KeyMaster
{
    /// <summary>
    /// Returns the decrypted form of the EncryptedText parameter string.
    /// </summary>
    /// <param name="EncryptedText"></param>
    /// <param name="PrivateKey"></param>
    /// <returns></returns>
    public static string Decrypt(string EncryptedText)
    {
        try
        {
            using RSA rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(GetKeys().Private), out _);
            byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(EncryptedText), RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decryptedData);
        }
        catch (FormatException f)
        {
            Console.WriteLine(f);
            throw new ArgumentException();
        }
    }

    /// <summary>
    /// Returns the encrypted form of the PlainText parameter string.
    /// </summary>
    /// <param name="PlainText"></param>
    /// <param name="PublicKey"></param>
    /// <returns></returns>
    public static string Encrypt(string PlainText)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(GetKeys().Public), out _);
        byte[] encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(PlainText), RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(encryptedData);
    }

    /// <summary>
    /// Gets the current active encryption keys.
    /// </summary>
    /// <param name="Context"></param>
    /// <returns>Keys record instance</returns>
    public static Keys GetKeys()
    {
        // Try getting the keys from the environment
        var Keys = new Keys
        (
            Environment.GetEnvironmentVariable("PUBLIC")!,
            Environment.GetEnvironmentVariable("PRIVATE")!
        );

        int ExpDuration = Convert.ToInt32(
            Environment.GetEnvironmentVariable("ENC_KEY_DUR") ?? "30");

        DateTime Creation = DateTime.Parse(
            Environment.GetEnvironmentVariable("KEY_CREATION")! ?? "01/01/2000");
        DateTime Expiration = Creation.AddDays(ExpDuration);

        if (
            string.IsNullOrEmpty(Keys.Public) ||
            string.IsNullOrEmpty(Keys.Private) ||
            Expiration <= DateTime.UtcNow)
        {

            // Keys need to be obtained from the DB
            using var Context = DatabaseManager.GetActiveContext();
            try
            {
                Encryption EncryptionKeys = Context.EncryptionKeys
                    .OrderByDescending(e => e.Created)
                    .First();

                Creation = EncryptionKeys.Created;
                Expiration = Creation.AddDays(ExpDuration);
                if (Expiration <= DateTime.UtcNow) throw new Exception();

                // Keys have been created
                Keys = new Keys
                (
                    EncryptionKeys.Public,
                    EncryptionKeys.Private
                );
            }
            catch
            {
                // Keys not created, create them
                using RSA rsa = RSA.Create(2048);
                Keys = new Keys
                (
                    Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()),
                    Convert.ToBase64String(rsa.ExportPkcs8PrivateKey())
                );

                var EncKeys = new Encryption
                {
                    Created = DateTime.UtcNow,
                    Public = Keys.Public,
                    Private = Keys.Private
                };

                Context.EncryptionKeys.Add(EncKeys);
                Context.SaveChanges();
                Console.WriteLine("New encryption keys have been created.");
                
                Creation = EncKeys.Created;
            }

            // Keys have been obtained, save them
            Environment.SetEnvironmentVariable("PUBLIC", Keys.Public);
            Environment.SetEnvironmentVariable("PRIVATE", Keys.Private);
            Environment.SetEnvironmentVariable("KEY_CREATION", Creation.ToShortTimeString());
        }

        return Keys;
    }
}