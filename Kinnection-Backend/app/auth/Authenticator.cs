using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Kinnection
{
    public static class Authenticator
    {
        private static readonly string EncodedHeader = Base64UrlEncoder.Encode(
            "{\"alg\":\"RS256\",\"typ\":\"JWT\"}");
        private static readonly string ISSUER = Environment.GetEnvironmentVariable("ISSUER") +
                Environment.GetEnvironmentVariable("ASP_PORT");
        private static readonly byte[] KEY = Encoding.UTF8.GetBytes(
            Environment.GetEnvironmentVariable("KEY")!);

        /// <summary>
        /// Authenticates and places refreshed tokens into the HttpContext if SaveHeaders is true.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="UserID"></param>
        /// <param name="SaveHeaders"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="AuthenticationException"></exception>
        public static async Task Authenticate(
            KinnectionContext Context,
            HttpContext httpContext,
            bool SaveHeaders = true)
        {
            var Keys = KeyMaster.SearchKeys();

            // Process and verify tokens
            var Access = KeyMaster.ProcessToken(httpContext.Request.Headers.Authorization!, Keys.Public);
            var Refresh = KeyMaster.ProcessToken(httpContext.Request.Headers["X-Refresh-Token"]!, Keys.Public);

            string AccessUser = Access["payload"]["sub"];
            string RefreshUser = Refresh["payload"]["sub"];

            if (AccessUser != RefreshUser)
                throw new AuthenticationException("Tokens do not match.");

            int UserID = Convert.ToInt32(AccessUser);

            var Auth = await Context.Authentications.FirstOrDefaultAsync(b => b.UserID == UserID)
                ?? throw new KeyNotFoundException($"User with ID {UserID} is not logged in.");

            // Verify access token matches user
            if (!KeyMaster.VerifySigning(
                Base64UrlEncoder.DecodeBytes(Access["signature"]["signature"]),
                Base64UrlEncoder.DecodeBytes(Auth.Authorization),
                Keys.Public)
            ) throw new AuthenticationException("Access denied.");

            // Verify refresh token is not previous refresh token
            if (KeyMaster.VerifySigning(
                Base64UrlEncoder.DecodeBytes(Refresh["signature"]["signature"]),
                Base64UrlEncoder.DecodeBytes(Auth.PrevRef),
                Keys.Public))
            {
                // Invalidate the tokens, unauthorized access attempt occurred
                Auth.Authorization = GenerateRandomString(64);
                Auth.Refresh = GenerateRandomString(64);
                Auth.PrevRef = GenerateRandomString(64);
                throw new AuthenticationException("Access denied.");
            }

            bool RefMatchesUser = KeyMaster.VerifySigning(
                Base64UrlEncoder.DecodeBytes(Refresh["signature"]["signature"]),
                Base64UrlEncoder.DecodeBytes(Auth.Refresh),
                Keys.Public);

            bool RefIsExpired = IsExpired(
                DateTimeOffset.FromUnixTimeSeconds(
                    Convert.ToInt64(Refresh["payload"]["exp"])
                ));

            bool AccIsExpired = IsExpired(
                DateTimeOffset.FromUnixTimeSeconds(
                    Convert.ToInt64(Access["payload"]["exp"])
                )
            );

            string AccessToken = "";
            string RefreshToken = "";

            // Verify access token is valid
            if (AccIsExpired)
            {
                if (!RefMatchesUser || RefIsExpired)
                    throw new AuthenticationException("Re-authentication required.");
                AccessToken = SignToken(GenerateAccessPayload(UserID));
                Auth.Authorization = AccessToken.Split('.')[2];
            }

            // Verify refresh token is valid and unexpired
            if (RefMatchesUser && !RefIsExpired)
            { // Refresh
                Auth.PrevRef = Auth.Refresh;
                RefreshToken = SignToken(GenerateRefreshPayload(UserID));
                Auth.Refresh = RefreshToken.Split('.')[2];
            }
            else
            { // Invalidate
                Auth.Refresh = GenerateRandomString(64);
                Auth.PrevRef = GenerateRandomString(64);
            }

            if (SaveHeaders)
            {
                httpContext.Response.Headers.Authorization = SignToken(Auth.Authorization);
                httpContext.Response.Headers["X-Refresh-Token"] = SignToken(Auth.Refresh);
            }
        }

        /// <summary>
        /// Returns true if PlainText, when hashed, is equivalent to the Hash parameter.
        /// </summary>
        /// <param name="Hash"></param>
        /// <param name="PlainText"></param>
        /// <returns></returns>
        public static bool CheckPasswordEquivalence(string Hash, string PlainText)
        {
            // TO DO: Use slower hash for better password management;
            //  separate out to PasswordManager module
            using var HMAC = new HMACSHA256(KEY);
            return CryptographicOperations.FixedTimeEquals(
                HMAC.ComputeHash(Encoding.UTF8.GetBytes(PlainText)),
                Encoding.UTF8.GetBytes(Hash)
            );
        }

        /// <summary>
        /// Returns a new Access Token Payload for user of UserID.
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        private static string GenerateAccessPayload(int UserID)
        {
            long IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(
                    Convert.ToDouble(Environment.GetEnvironmentVariable("ACCESS_DURATION"))
                ).ToUnixTimeSeconds();

            return JsonSerializer.Serialize(
                new
                {
                    iss = ISSUER,
                    sub = UserID,
                    exp = ExpiresAt,
                    iat = IssuedAt
                });
        }

        /// <summary>
        /// Returns a new Refresh Token Payload for user of UserID.
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        private static string GenerateRefreshPayload(int UserID)
        {
            long IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long ExpiresAt = DateTimeOffset.UtcNow.AddDays(
                    Convert.ToDouble(Environment.GetEnvironmentVariable("REFRESH_DURATION"))
                ).ToUnixTimeSeconds();
            string SessionID = GenerateRandomString();

            return JsonSerializer.Serialize(
                new
                {
                    iss = ISSUER,
                    sub = UserID,
                    exp = ExpiresAt,
                    iat = IssuedAt,
                    sid = SessionID
                });
        }

        /// <summary>
        /// Returns a new randomly generated string with specified length.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private static string GenerateRandomString(int length = 24)
        {
            return RandomNumberGenerator.GetString(
                [
                    'A','B','C','D','E','F','G','H','I','J','K','L','M',
                    'N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
                    'a','b','c','d','e','f','g','h','i','j','k','l','m',
                    'n','o','p','q','r','s','t','u','v','w','x','y','z',
                    '0','1','2','3','4','5','6','7','8','9','#','@','$'
                ],
                length
            );
        }

        /// <summary>
        /// Returns true if token is not expired, otherwise false
        /// </summary>
        /// <param name="ExpirationTime"></param>
        /// <returns></returns>
        private static bool IsExpired(DateTimeOffset ExpirationTime)
        {
            return DateTimeOffset.Compare(
                ExpirationTime,
                DateTimeOffset.UtcNow
            ) <= 0;
        }

        /// <summary>
        /// Provisions a new set of tokens to the user of UserID, accessible with keys "access" and "refresh".
        /// Only use this function when a successful login occurs to issue a user brand new tokens.
        /// </summary>
        /// <param name="UserID"></param>
        public static async Task<Dictionary<string, string>> Provision(int UserID)
        {
            using var Context = DatabaseManager.GetActiveContext();
            var Auth = await Context.Authentications.FirstOrDefaultAsync(b => b.UserID == UserID);

            string AccessPayload = GenerateAccessPayload(UserID);
            string RefreshPayload = GenerateRefreshPayload(UserID);

            if (Auth == null)
            {
                Auth = new Authentication
                {
                    Created = DateTime.UtcNow,
                    UserID = UserID,
                    Authorization = AccessPayload,
                    Refresh = RefreshPayload,
                    PrevRef = ""
                };
                Context.Add(Auth);
            }
            else
            {
                Auth.Authorization = AccessPayload;
                Auth.Refresh = RefreshPayload;
            }

            await Context.SaveChangesAsync();

            var tokens = new Dictionary<string, string>
            {
                ["access"] = SignToken(Auth.Authorization),
                ["refresh"] = SignToken(Auth.Refresh)
            };

            return await Task.FromResult(tokens);
        }

        /// <summary>
        /// Returns the signed and encrypted token.
        /// </summary>
        /// <param name="Payload"></param>
        /// <returns></returns>
        private static string SignToken(string Payload)
        {
            string PrivateKey = Environment.GetEnvironmentVariable("private")!;
            string EncodedPayload = Base64UrlEncoder.Encode(Payload);
            string EncryptedSignature = KeyMaster.Sign($"{EncodedHeader}.{EncodedPayload}", PrivateKey);
            return $"{EncodedHeader}.{EncodedPayload}.{EncryptedSignature}";
        }
    }
}