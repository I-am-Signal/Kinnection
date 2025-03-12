using System.Text;
using System.Security.Cryptography;

namespace Kinnection
{
    public static class KeyMaster
    {
        /// <summary>
        /// 
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
        
        public static string EncryptWithPublicKey(string PlainText, string publicKey)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
            byte[] encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(PlainText), RSAEncryptionPadding.OaepSHA256);
            return Convert.ToBase64String(encryptedData);
        }

        public static string DecryptWithPrivateKey(string EncryptedText, string privateKey)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
            byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(EncryptedText), RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}