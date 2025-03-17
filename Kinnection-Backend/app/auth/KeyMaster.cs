using System.Text;
using System.Security.Cryptography;

namespace Kinnection
{
    public static class KeyMaster
    {
        /// <summary>
        /// Generates and returns a Dictionary containing "private" and "public" encryption keys.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GenerateKeys()
        {
            using RSA rsa = RSA.Create(2048);
            return new Dictionary<string, string> 
            {
                ["public"] = Convert.ToBase64String(rsa.ExportRSAPublicKey()),
                ["private"] = Convert.ToBase64String(rsa.ExportRSAPrivateKey())
            };
        }

        /// <summary>
        /// Sets the "Public" and "Private" encryption key environment variables.
        /// </summary>
        /// <param name="Public"></param>
        /// <param name="Private"></param>
        public static void SetKeys(string Public, string Private)
        {
            Environment.SetEnvironmentVariable("Public", Public);
            Environment.SetEnvironmentVariable("Private", Private);
        }
        
        /// <summary>
        /// Returns the encrypted form of the PlainText parameter string.
        /// </summary>
        /// <param name="PlainText"></param>
        /// <param name="PublicKey"></param>
        /// <returns></returns>
        public static string EncryptWithPublicKey(string PlainText, string PublicKey)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(PublicKey), out _);
            byte[] encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(PlainText), RSAEncryptionPadding.OaepSHA256);
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Returns the decrypted form of the EncryptedText parameter string.
        /// </summary>
        /// <param name="EncryptedText"></param>
        /// <param name="PrivateKey"></param>
        /// <returns></returns>
        public static string DecryptWithPrivateKey(string EncryptedText, string PrivateKey)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(PrivateKey), out _);
            byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(EncryptedText), RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}