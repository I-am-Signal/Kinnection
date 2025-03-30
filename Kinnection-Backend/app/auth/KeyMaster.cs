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
    public static string Decrypt(string EncryptedText, string PrivateKey)
    {
        try
        {
            using RSA rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(PrivateKey), out _);
            byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(EncryptedText), RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decryptedData);
        }
        catch (FormatException) { throw new ArgumentException(); }
    }

    /// <summary>
    /// Returns the encrypted form of the PlainText parameter string.
    /// </summary>
    /// <param name="PlainText"></param>
    /// <param name="PublicKey"></param>
    /// <returns></returns>
    public static string Encrypt(string PlainText, string PublicKey)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(PublicKey), out _);
        byte[] encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(PlainText), RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(encryptedData);
    }

    /// <summary>
    /// Returns a Keys record.
    /// </summary>
    /// <param name="Context"></param>
    /// <returns>Keys record instance</returns>
    public static Keys GetKeys()
    {
        // Try getting the keys from the environment
        var Keys = new Keys
        (
            Environment.GetEnvironmentVariable("public")!,
            Environment.GetEnvironmentVariable("private")!
        );

        if (string.IsNullOrEmpty(Keys.Public) || string.IsNullOrEmpty(Keys.Private))
        {
            // Keys need to be obtained from the DB
            using var Context = DatabaseManager.GetActiveContext();
            try
            {
                Encryption EncryptionKeys = Context.EncryptionKeys
                    .OrderByDescending(e => e.Created)
                    .First();

                // Keys have been created
                Keys = new Keys
                (
                    EncryptionKeys.Public,
                    EncryptionKeys.Private
                );
            }
            catch (Exception)
            {
                // Keys not created, create them
                using RSA rsa = RSA.Create(2048);
                Keys = new Keys
                (
                    Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()),
                    Convert.ToBase64String(rsa.ExportPkcs8PrivateKey())
                );

                Context.Add(new Encryption
                {
                    Created = DateTime.UtcNow,
                    Public = Keys.Public,
                    Private = Keys.Private
                });
                Context.SaveChanges();
                Console.WriteLine("New encryption keys have been created.");
            }

            // Keys have been obtained, save them
            Environment.SetEnvironmentVariable("public", Keys.Public);
            Environment.SetEnvironmentVariable("private", Keys.Private);
        }

        return Keys;
    }
}