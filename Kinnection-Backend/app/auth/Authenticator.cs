using Microsoft.IdentityModel.Tokens;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Kinnection;

public static class Authenticator
{
    private static readonly string EncodedHeader = Base64UrlEncoder.Encode(
        "{\"alg\":\"RS256\",\"typ\":\"JWT\"}");
    private static readonly string ISSUER = Environment.GetEnvironmentVariable("ISSUER") +
        ':' + Environment.GetEnvironmentVariable("ASP_PORT");

    /// <summary>
    /// Authenticates access and refresh tokens. 
    /// Places refreshed tokens into httpContext if it's provided.
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="httpContext"></param>
    /// <param name="Tokens"></param>
    /// <returns>Dictionary with "access" and "refresh" tokens</returns>
    /// <exception cref="AuthenticationException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    public static Dictionary<string, string> Authenticate(
        KinnectionContext Context,
        Dictionary<string, string>? Tokens = null,
        HttpContext? httpContext = null
        )
    {
        var Keys = KeyMaster.GetKeys();

        string RawAccess, RawRefresh;
        // Process and verify tokens
        if (httpContext != null)
        {
            RawAccess = httpContext.Request.Headers.Authorization!.ToString().Split(" ")[1];
            RawRefresh = httpContext.Request.Headers["X-Refresh-Token"]!;
        }
        else if (Tokens != null)
        {
            RawAccess = Tokens["access"];
            RawRefresh = Tokens["refresh"];
        }
        else throw new AuthenticationException("No tokens provided for authentication!");

        var Access = ProcessToken(RawAccess);
        var Refresh = ProcessToken(RawRefresh);

        int UserID = Access["payload"]["sub"].GetInt32();
        int RefreshUserID = Refresh["payload"]["sub"].GetInt32();

        if (UserID != RefreshUserID)
            throw new AuthenticationException("Tokens do not match.");

        var Auth = Context.Authentications.FirstOrDefault(b => b.UserID == UserID)
            ?? throw new KeyNotFoundException($"User with ID {UserID} is not logged in.");

        // Verify access token matches user
        if (!IsEqual(
            Access["signature"]["signature"].GetString()!,
            Auth.Authorization)
        ) throw new AuthenticationException("Access denied.");

        // Verify refresh token is not previous refresh token
        if (IsEqual(
            Refresh["signature"]["signature"].GetString()!,
            Auth.PrevRef))
        {
            // Invalidate the tokens, unauthorized access attempt occurred
            Auth.Authorization = GenerateRandomString(64);
            Auth.Refresh = GenerateRandomString(64);
            Auth.PrevRef = GenerateRandomString(64);
            throw new AuthenticationException("Access denied.");
        }

        bool RefMatchesUser = IsEqual(
            Refresh["signature"]["signature"].GetString()!,
            Auth.Refresh);

        bool RefIsExpired = IsExpired(
            DateTimeOffset.FromUnixTimeSeconds(
                Convert.ToInt64(Refresh["payload"]["exp"].GetInt64())
            ));

        bool AccIsExpired = IsExpired(
            DateTimeOffset.FromUnixTimeSeconds(
                Convert.ToInt64(Access["payload"]["exp"].GetInt64())
            )
        );

        string AccessToken = RawAccess; // Save access if not being refreshed
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

        if (httpContext != null)
        {
            httpContext.Response.Headers.Authorization = $"Bearer {AccessToken}";
            httpContext.Response.Headers["X-Refresh-Token"] = RefreshToken;
        }

        return new Dictionary<string, string>
        {
            ["access"] = AccessToken,
            ["refresh"] = RefreshToken
        };
    }

    /// <summary>
    /// Returns true if strings are equal (in a cryptographically fixed time), false otherwise.
    /// </summary>
    /// <param name="String1"></param>
    /// <param name="String2"></param>
    /// <returns></returns>
    public static bool IsEqual(string String1, string String2)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(String1),
            Encoding.UTF8.GetBytes(String2));
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
    /// Returns the verified token in a Dictionary with keys of "header", "payload", and "signature"
    /// </summary>
    /// <param name="Token"></param>
    /// <param name="PublicKey"></param>
    /// <returns></returns>
    /// <exception cref="AuthenticationException"></exception>
    public static Dictionary<string, Dictionary<string, JsonElement>> ProcessToken(
        string Token)
    {
        if (!VerifyToken(Token))
            throw new AuthenticationException("Token has been tampered.");

        string[] TokenParts = Token.Split('.');
        if (TokenParts.Length != 3)
            throw new AuthenticationException("Invalid token format.");

        try
        {
            string DecodedHeader = Base64UrlEncoder.Decode(TokenParts[0]);
            string DecodedPayload = Base64UrlEncoder.Decode(TokenParts[1]);
            string EncodedSignature = TokenParts[2]; // easier to use via decoding at arrival

            var JsonSignature = new { signature = (string) EncodedSignature };

            var Header = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(DecodedHeader)!;
            var Payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(DecodedPayload)!;
            var Signature = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                JsonSerializer.Serialize(JsonSignature))!;

            return new Dictionary<string, Dictionary<string, JsonElement>>
            {
                ["header"] = Header,
                ["payload"] = Payload,
                ["signature"] = Signature
            };
        }
        catch (JsonException j)
        {
            Console.WriteLine(j);
            throw new AuthenticationException("Invalid token format.");
        }
    }

    /// <summary>
    /// Provisions a new set of tokens to the user of UserID.
    /// If provided an HttpContext, the tokens are saved to the Response headers.
    /// Only use this function when a successful login occurs to issue a user brand new tokens. 
    /// </summary>
    /// <param name="UserID"></param>
    /// <param name="httpContext"></param>
    /// <returns>Dictionary of tokens accessible with keys "access" and "refresh".</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    public static Dictionary<string, string> Provision(
        int UserID,
        HttpContext? httpContext = null)
    {
        using var Context = DatabaseManager.GetActiveContext();

        // Get the existing user to ensure that this is being created for a real user
        var ExistingUser = Context.Users.FirstOrDefault(u => u.ID == UserID);
        var Auth = Context.Authentications.FirstOrDefault(b => b.UserID == UserID);

        string AccessToken = SignToken(GenerateAccessPayload(UserID));
        string RefreshToken = SignToken(GenerateRefreshPayload(UserID));
        string AccessHash = AccessToken.Split('.')[2];
        string RefreshHash = RefreshToken.Split('.')[2];

        if (ExistingUser == null)
        {
            throw new KeyNotFoundException($"User {UserID} does not exist.");
        }
        else if (Auth == null)
        {
            Auth = new Authentication
            {
                Created = DateTime.UtcNow,
                UserID = UserID,
                Authorization = AccessHash,
                Refresh = RefreshHash,
                PrevRef = ""
            };
            Context.Add(Auth);
        }
        else
        {
            Auth.Authorization = AccessHash;
            Auth.Refresh = RefreshHash;
        }

        Context.SaveChanges();

        if (httpContext != null)
        {
            httpContext.Response.Headers.Authorization = $"Bearer {AccessToken}";
            httpContext.Response.Headers["X-Refresh-Token"] = RefreshToken;
        }

        return new Dictionary<string, string>
        {
            ["access"] = AccessToken,
            ["refresh"] = RefreshToken
        };
    }

    /// <summary>
    /// Returns the signed and encrypted token.
    /// </summary>
    /// <param name="Payload"></param>
    /// <returns></returns>
    private static string SignToken(string Payload)
    {
        // Encode token parts
        string EncodedPayload = Base64UrlEncoder.Encode(Payload);

        using RSA rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(
            KeyMaster.GetKeys().Private), out _);
            
        string EncryptedSignature = Base64UrlEncoder.Encode(rsa.SignData(
            Encoding.UTF8.GetBytes($"{EncodedHeader}.{EncodedPayload}"),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1));
        
        // Compile and return token
        return $"{EncodedHeader}.{EncodedPayload}.{EncryptedSignature}";
    }

    /// <summary>
    /// Verifies that the JWT is untampered.
    /// </summary>
    /// <param name="Token"></param>
    /// <param name="PublicKey"></param>
    /// <returns>Returns true if the signed JWT is untampered, false otherwise.</returns>
    public static bool VerifyToken(string Token)
    {
        try
        {
            var Keys = KeyMaster.GetKeys();

            string[] TokenParts = Token.Split('.');
            if (TokenParts.Length != 3) return false;

            byte[] EncodedMessage = Encoding.UTF8.GetBytes($"{TokenParts[0]}.{TokenParts[1]}");
            byte[] DecodedSignature = Base64UrlEncoder.DecodeBytes(TokenParts[2]);

            using RSA rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(Keys.Public), out _);

            return rsa.VerifyData(
                EncodedMessage,
                DecodedSignature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new AuthenticationException("Invalid token provided.");
        }
    }
}