using System.Text;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Kinnection
{
    public static class KeyMaster
    {
        /// <summary>
        /// Used for signing JWTs
        /// </summary>
        private static readonly byte[] EncryptionKey = Convert.FromBase64String(Environment.GetEnvironmentVariable("encrypt")!);

        /// <summary>
        /// Generates and returns a Dictionary containing "private" and "public" encryption keys.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GenerateKeys(
            int keySizeInBits = 2048)
        {
            using RSA rsa = RSA.Create(keySizeInBits);
            return new Dictionary<string, string>
            {
                ["public"] = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()),
                ["private"] = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey())
            };
        }

        /// <summary>
        /// Sets environment variables corresponding with the key-value pairs in the Keys dictionary.
        /// </summary>
        /// <param name="Keys"></param>
        public static void SetKeys(string Public, string Private)
        {
            Environment.SetEnvironmentVariable("public", Public, EnvironmentVariableTarget.Machine);
            Environment.SetEnvironmentVariable("private", Private, EnvironmentVariableTarget.Machine);
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
        /// Returns the decrypted form of the EncryptedText parameter string.
        /// </summary>
        /// <param name="EncryptedText"></param>
        /// <param name="PrivateKey"></param>
        /// <returns></returns>
        public static string Decrypt(string EncryptedText, string PrivateKey)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(PrivateKey), out _);
            byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(EncryptedText), RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decryptedData);
        }

        /// <summary>
        /// Returns the Base64Url encoded signed text
        /// </summary>
        /// <param name="SignableText"></param>
        /// <returns></returns>
        public static string Sign(string SignableText, string PrivateKey)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(PrivateKey), out _);
            return Base64UrlEncoder.Encode(rsa.SignData(
                Encoding.UTF8.GetBytes(SignableText),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1));
        }

        /// <summary>
        /// Returns true if the signed JWT is untampered
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="PublicKey"></param>
        /// <returns></returns>
        public static bool VerifyToken(string Token, string PublicKey)
        {
            string[] TokenParts = Token.Split('.');
            if (TokenParts.Length != 3) return false;

            byte[] EncodedMessage = Encoding.UTF8.GetBytes($"{TokenParts[0]}.{TokenParts[1]}");
            byte[] DecodedSignature = Base64UrlEncoder.DecodeBytes(TokenParts[2]);

            using RSA rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(PublicKey), out _);

            return rsa.VerifyData(
                EncodedMessage, 
                DecodedSignature, 
                HashAlgorithmName.SHA256, 
                RSASignaturePadding.Pkcs1);
        }
    }
}