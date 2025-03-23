using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using Microsoft.IdentityModel.Tokens;

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
            int UserID,
            bool SaveHeaders = true)
        {
            // Ensure user exists
            var ExistingUser = await Context.Users
                .FirstOrDefaultAsync(b => b.ID == UserID) ??
                    throw new KeyNotFoundException($"A user with id {UserID} does not exist.");

            // Decrypt tokens
            var PrivateKey = (await Context.EncryptionKeys.FirstOrDefaultAsync())!.Private;
            string Authorization = KeyMaster.Decrypt(
                httpContext.Request.Headers.Authorization!,
                PrivateKey
            );
            Authorization = Authorization.Split(" ", 2)[1]; // remove 'Bearer'

            string Refresh = KeyMaster.Decrypt(
                httpContext.Request.Headers["X-Refresh-Token"]!,
                PrivateKey
            );

            // Ensure tokens match user
            var Tokens = await Check(Authorization, Refresh, UserID);
            if (SaveHeaders)
            {
                httpContext.Response.Headers.Authorization = Tokens["access"];
                httpContext.Response.Headers["X-Refresh-Token"] = Tokens["refresh"];
            }
        }

        /// <summary>
        /// Verifies and refreshes tokens of user with UserID (if needed), returning
        /// a Dictionary with tokens accessible at keys "access" and "refresh".
        /// </summary>
        /// <param name="Access"></param>
        /// <param name="Refresh"></param>
        /// <param name="UserID"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="AuthenticationException"></exception>
        public static async Task<Dictionary<string, string>> Check(string Access, string Refresh, int UserID)
        {
            string PrivateKey = Environment.GetEnvironmentVariable("private")!;

            using var context = DatabaseManager.GetActiveContext();
            var Auth = await context.Authentications.FirstOrDefaultAsync(b => b.UserID == UserID)
                ?? throw new KeyNotFoundException($"User with ID {UserID} is not logged in.");

            // Verify access token is current access token
            if (!KeyMaster.VerifyToken(Access, PrivateKey))
                throw new AuthenticationException("Invalid access token.");

            // Verify refresh token is not previous refresh token
            if (IsPreviousRefresh(Refresh, Auth.PrevRef))
            {
                // Invalidate the tokens, unauthorized access attempt occurred
                Auth.Authorization = GenerateRandomString(64);
                Auth.Refresh = GenerateRandomString(64);
                Auth.PrevRef = GenerateRandomString(64);
                throw new AuthenticationException("Access denied.");
            }

            bool ValidRefresh = !KeyMaster.VerifyToken(Refresh, PrivateKey) && IsExpired(Auth.Refresh);

            // Verify access token is valid
            if (IsExpired(Auth.Authorization))
            {
                if (!ValidRefresh)
                    throw new AuthenticationException("Re-authentication required.");
                Auth.Authorization = GenerateAccessPayload(UserID);
            }

            // Verify refresh token is valid and unexpired
            if (ValidRefresh)
            { // Refresh
                Auth.PrevRef = Auth.Refresh;
                Auth.Refresh = GenerateRefreshPayload(UserID);
            }
            else
            { // Invalidate
                Auth.Refresh = GenerateRandomString(64);
                Auth.PrevRef = GenerateRandomString(64);
            }

            return await Task.FromResult(
                new Dictionary<string, string>()
                {
                    {"access", SignToken(Auth.Authorization)},
                    {"refresh", SignToken(Auth.Refresh)}
                }
            );
        }

        /// <summary>
        /// Returns true if PlainText, when hashed, is equivalent to the Hash parameter.
        /// </summary>
        /// <param name="Hash"></param>
        /// <param name="PlainText"></param>
        /// <returns></returns>
        public static bool CheckHashEquivalence(string Hash, string PlainText)
        {
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
        /// Returns a new randomly generated Session ID.
        /// </summary>
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
        /// <param name="Token"></param>
        /// <returns></returns>
        private static bool IsExpired(string Token)
        {
            Dictionary<string, string> ParsedToken;
            try
            {
                ParsedToken = JsonSerializer.Deserialize<Dictionary<string, string>>(Token)!;
            }
            catch (JsonException)
            {
                throw new AuthenticationException("Invalid token.");
            }

            return DateTimeOffset.Compare(
                    DateTimeOffset.FromUnixTimeSeconds(
                        Convert.ToInt64(ParsedToken!["exp"])
                    ),
                    DateTimeOffset.UtcNow
                ) <= 0;
        }


        private static bool IsPreviousRefresh(string Refresh, string PrevRefPayload)
        {
            string[] TokenParts = Refresh.Split('.');
            if (TokenParts.Length != 3) return false;

            string DecodedPayload = Base64UrlEncoder.Decode(TokenParts[1]);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(DecodedPayload),
                Encoding.UTF8.GetBytes(PrevRefPayload)
            );
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