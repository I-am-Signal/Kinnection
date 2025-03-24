using Microsoft.IdentityModel.Tokens;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Kinnection
{
    public record Keys
    (
        string Public,
        string Private
    );

    public static class KeyMaster
    {
        /// <summary>
        /// Generates and returns a Keys record.
        /// </summary>
        /// <returns></returns>
        private static Keys GenerateKeys()
        {
            using RSA rsa = RSA.Create(2048);
            return new Keys
            (
                Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()),
                Convert.ToBase64String(rsa.ExportPkcs8PrivateKey())
            );
        }

        /// <summary>
        /// Returns a Keys record.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns>Keys</returns>
        public static Keys SearchKeys()
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
                    // Keys have not been created yet
                    Keys = GenerateKeys();
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
                Environment.SetEnvironmentVariable("public", Keys.Public, EnvironmentVariableTarget.Machine);
                Environment.SetEnvironmentVariable("private", Keys.Private, EnvironmentVariableTarget.Machine);
            }

            return Keys;
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

            return VerifySigning(EncodedMessage, DecodedSignature, PublicKey);
        }

        /// <summary>
        /// Returns true if signature hashes are the same, false otherwise
        /// </summary>
        /// <param name="Hash1"></param>
        /// <param name="Hash2"></param>
        /// <param name="PublicKey"></param>
        /// <returns></returns>
        public static bool VerifySigning(byte[] Hash1, byte[] Hash2, string PublicKey)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(PublicKey), out _);

            return rsa.VerifyData(
                Hash1,
                Hash2,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// Returns the verified token in a Dictionary with keys of "header", "payload", and "signature"
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="PublicKey"></param>
        /// <returns></returns>
        /// <exception cref="AuthenticationException"></exception>
        public static Dictionary<string, Dictionary<string, string>> ProcessToken(
            string Token,
            string PublicKey)
        {
            if (!VerifyToken(Token, PublicKey))
                throw new AuthenticationException("Token has been tampered.");

            string[] TokenParts = Token.Split('.');
            if (TokenParts.Length != 3)
                throw new AuthenticationException("Invalid token format.");

            try
            {
                string DecodedHeader = Base64UrlEncoder.Decode(TokenParts[0]);
                string DecodedPayload = Base64UrlEncoder.Decode(TokenParts[1]);
                string DecodedSignature = Base64UrlEncoder.Decode(TokenParts[2]);
                return new Dictionary<string, Dictionary<string, string>>
                {
                    ["header"] = JsonSerializer.Deserialize<Dictionary<string, string>>(DecodedHeader)!,
                    ["payload"] = JsonSerializer.Deserialize<Dictionary<string, string>>(DecodedPayload)!,
                    ["signature"] = JsonSerializer.Deserialize<Dictionary<string, string>>(
                        $"{{\"signature\":\"{DecodedSignature}\"}}")!
                };
            }
            catch (JsonException) { throw new AuthenticationException("Invalid token format."); }
        }
    }
}