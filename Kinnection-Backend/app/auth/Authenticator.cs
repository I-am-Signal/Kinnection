using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;

namespace Kinnection
{
    public static class Authenticator
    {
        private static readonly byte[] SECRET = Encoding.UTF8.GetBytes(
            Environment.GetEnvironmentVariable("SECRET")!);
        private static readonly byte[] KEY = Encoding.UTF8.GetBytes(
            Environment.GetEnvironmentVariable("KEY")!);

        private static readonly string ISSUER = Environment.GetEnvironmentVariable("ISSUER") +
                Environment.GetEnvironmentVariable("ASP_PORT");
        private static readonly byte[] JOSE = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(
                new JWTHeader(
                    "HS256",
                    "JWT"
                )));
        private record JWTHeader
        (
            [property: JsonPropertyName("alg")] string Algorithm,
            [property: JsonPropertyName("typ")] string Type
        );

        private record JWTAccessPayload
        (
            [property: JsonPropertyName("iss")] string Issuer,
            [property: JsonPropertyName("sub")] int Subject,
            [property: JsonPropertyName("exp")] long Expiration,
            [property: JsonPropertyName("iat")] long Issued
        );

        private record JWTRefreshPayload
        (
            [property: JsonPropertyName("iss")] string Issuer,
            [property: JsonPropertyName("sub")] int Subject,
            [property: JsonPropertyName("exp")] long Expiration,
            [property: JsonPropertyName("iat")] long Issued,
            [property: JsonPropertyName("sid")] string SessionID
        );

        /// <summary>
        /// Provisions a new set of tokens to the user of UserID. 
        /// Only use this function when a successful login occurs to issue a user brand new tokens.
        /// </summary>
        /// <param name="UserID"></param>
        public static async Task<string[]> Provision(int UserID)
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

            string[] tokens = [
                HashToken(Auth.Authorization),
                HashToken(Auth.Refresh)
            ];

            return await Task.FromResult(tokens);
        }

        /// <summary>
        /// Verifies and refreshes tokens of user with UserID (if needed), returning
        /// a string array containing, in order, the access and refresh tokens.
        /// </summary>
        /// <param name="Access"></param>
        /// <param name="Refresh"></param>
        /// <param name="UserID"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="AuthenticationException"></exception>
        public static async Task<Dictionary<string, string>> Check(string Access, string Refresh, int UserID)
        {
            using var context = DatabaseManager.GetActiveContext();
            var Auth = await context.Authentications.FirstOrDefaultAsync(b => b.UserID == UserID)
                ?? throw new KeyNotFoundException();

            // Verify access token is current access token
            if (!CheckHashEquivalence(Access, Auth.Authorization))
                throw new AuthenticationException("Invalid access token.");

            // Verify refresh token is not previous refresh token
            if (CheckHashEquivalence(Refresh, Auth.PrevRef))
            {
                // Invalidate the tokens, unauthorized access attempt occurred
                Auth.Authorization = GenerateRandomString(64);
                Auth.Refresh = GenerateRandomString(64);
                Auth.PrevRef = GenerateRandomString(64);
                throw new AuthenticationException("Access denied.");
            }

            bool ValidRefresh = CheckHashEquivalence(Refresh, Auth.Refresh) && IsExpired(Auth.Refresh);

            // Verify access token is valid
            if (IsExpired(Auth.Authorization))
            {
                if (!ValidRefresh)
                    throw new AuthenticationException("Access token expired. Re-authentication required.");
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

            Dictionary<string, string> tokens = new Dictionary<string, string>()
            {
                {"Authorization", HashToken(Auth.Authorization)},
                {"Refresh", HashToken(Auth.Refresh)}
            };

            return await Task.FromResult(tokens);
        }

        /// <summary>
        /// Verifies and refreshes tokens of user with UserID (if needed), returning
        /// a string array containing, in order, the access and refresh tokens.
        /// </summary>
        /// <param name="Access"></param>
        /// <param name="Refresh"></param>
        /// <param name="UserID"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="AccessViolationException"></exception>
        private static async Task<Dictionary<string, string>> Refresh(string Access, string Refresh, int UserID)
        {
            using var context = DatabaseManager.GetActiveContext();
            var Auth = await context.Authentications.FirstOrDefaultAsync(b => b.UserID == UserID)
                ?? throw new KeyNotFoundException();

            // Verify access token is valid
            bool AccessIsValid = CheckHashEquivalence(Access, Auth.Authorization);

            // Verify refresh token is not the previous one
            bool RefreshIsPrevious = CheckHashEquivalence(Refresh, Auth.PrevRef);

            // Verify refresh token is valid
            bool RefreshIsValid = CheckHashEquivalence(Access, Auth.Refresh);

            // Update tokens (if necessary)
            Auth.PrevRef = Auth.Refresh;
            Auth.Authorization = GenerateAccessPayload(UserID);
            Auth.Refresh = GenerateRefreshPayload(UserID);

            Dictionary<string, string> tokens = new Dictionary<string, string>()
            {
                {"Authorization", HashToken(Auth.Authorization)},
                {"Refresh", HashToken(Auth.Refresh)}
            };

            return await Task.FromResult(tokens);
        }

        /// <summary>
        /// Compiles payload into a complete token and returns
        /// true if the hashes match, otherwise false.
        /// </summary>
        /// <param name="Hash"></param>
        /// <param name="Payload"></param>
        /// <returns></returns>
        private static bool CheckHashEquivalence(string Hash, string Payload)
        {
            // Compile the token from the saved source
            string JWTCompiled = HashToken(Payload);

            // Verify tokens are the same
            if (JWTCompiled.Length != Hash.Length)
            {
                Console.WriteLine("Token mismatch");
                return false;
            }

            for (int i = 0; i < Hash.Length; i++)
            {
                if (JWTCompiled[i] != Hash[i])
                {
                    Console.WriteLine("Token mismatch");
                    return false;
                }
            }
            Console.WriteLine("Tokens match");
            return true;
        }

        /// <summary>
        /// Returns a new randomly generated Session ID.
        /// </summary>
        /// <returns></returns>
        private static string GenerateRandomString(int length = 24)
        {
            ;
            return RandomNumberGenerator.GetString(new Char[]
                {
                    'A','B','C','D','E','F','G','H','I','J','K','L','M',
                    'N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
                    'a','b','c','d','e','f','g','h','i','j','k','l','m',
                    'n','o','p','q','r','s','t','u','v','w','x','y','z',
                    '0','1','2','3','4','5','6','7','8','9','#','@','$'
                },
                length
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

            string Payload = JsonSerializer.Serialize(
                new JWTAccessPayload(
                    ISSUER,
                    UserID,
                    ExpiresAt,
                    IssuedAt
                ));

            return Payload;
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

            string Payload = JsonSerializer.Serialize(
                new JWTRefreshPayload(
                    ISSUER,
                    UserID,
                    ExpiresAt,
                    IssuedAt,
                    SessionID
                ));

            return Payload;
        }

        /// <summary>
        /// Returns the completed hashed token.
        /// </summary>
        /// <param name="Payload"></param>
        /// <returns></returns>
        private static string HashToken(string Payload)
        {
            using HMACSHA256 HMAC = new HMACSHA256(KEY);
            string Header = Convert.ToBase64String(
                HMAC.ComputeHash(JOSE)
            );

            Payload = Convert.ToBase64String(
                HMAC.ComputeHash(Encoding.UTF8.GetBytes(Payload))
            );

            string JWTSecret = Convert.ToBase64String(
                HMAC.ComputeHash(SECRET)
            );
            return Header + "." + Payload + "." + JWTSecret;
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
            catch (JsonException) { throw new AuthenticationException("Invalid token."); }

            return DateTimeOffset.Compare(
                    DateTimeOffset.FromUnixTimeSeconds(
                        Convert.ToInt64(ParsedToken!["Expiration"])
                    ),
                    DateTimeOffset.UtcNow
                ) <= 0;
        }
    }
}