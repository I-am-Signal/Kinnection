using Microsoft.EntityFrameworkCore;
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
    private static readonly string ISSUER = Environment.GetEnvironmentVariable("ISSUER")!;
    private static readonly string ASP_PORT = Environment.GetEnvironmentVariable("ASP_PORT")!;
    /// <summary>
    /// Adds the access and refresh tokens to the Set-Cookie header as 'Authorization' and 
    /// 'X-Refresh-Token' respectively.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="Access"></param>
    /// <param name="Refresh"></param>
    /// <exception cref="Exception"></exception>
    public static void AddHttpOnlyTokens(HttpContext httpContext, string Access, string Refresh)
    {
        string ENV = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
        string ACCESS_DURATION = Environment.GetEnvironmentVariable("ACCESS_DURATION")!;
        string REFRESH_DURATION = Environment.GetEnvironmentVariable("REFRESH_DURATION")!;
        if (string.IsNullOrWhiteSpace(ENV) || string.IsNullOrWhiteSpace(ACCESS_DURATION) || string.IsNullOrWhiteSpace(REFRESH_DURATION))
            throw new Exception("Authentication token environment variables are missing!");

        CookieOptions AccessOptions, RefreshOptions;

        if (ENV == "Production")
        {
            AccessOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(
                    Convert.ToInt32(ACCESS_DURATION)
                )
            };
            RefreshOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(
                    Convert.ToInt32(REFRESH_DURATION)
                )
            };
        }
        else
        {
            AccessOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(
                    Convert.ToInt32(ACCESS_DURATION))
            };
            RefreshOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(
                    Convert.ToInt32(REFRESH_DURATION))
            };
        }

        httpContext.Response.Cookies.Append(
            "Authorization", $"Bearer {Access}", AccessOptions);
        httpContext.Response.Cookies.Append(
            "X-Refresh-Token", Refresh, RefreshOptions);
    }

    /// <summary>
    /// Authenticates access and refresh tokens. 
    /// Places refreshed tokens into httpContext if it's provided.
    /// </summary>
    /// <param name="Context"></param>
    /// <param name="httpContext"></param>
    /// <param name="Tokens"></param>
    /// <returns>Dictionary with "access" token, "refresh" token, and "user_id" values</returns>
    /// <exception cref="AuthenticationException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    public static (Dictionary<string, string> Tokens, int UserID) Authenticate(
        KinnectionContext Context,
        Dictionary<string, string>? Tokens = null,
        HttpContext? httpContext = null
        )
    {
        var Keys = KeyMaster.GetKeys();

        string RawAccess, RawRefresh;
        // Process and verify tokens
        if (httpContext != null) (RawAccess, RawRefresh) = ProcessCookies(httpContext);
        else if (Tokens != null) (RawAccess, RawRefresh) = (Tokens["access"], Tokens["refresh"]);
        else throw new AuthenticationException("No tokens provided for authentication!");

        var Access = ProcessToken(RawAccess);
        ValidateTokenClaims(Access);

        var Refresh = ProcessToken(RawRefresh);
        ValidateTokenClaims(Refresh);

        int UserID = Access["payload"]["sub"].GetInt32();
        int RefreshUserID = Refresh["payload"]["sub"].GetInt32();

        if (UserID != RefreshUserID)
            throw new AuthenticationException("Tokens do not match.");

        var Auth = Context.Authentications.FirstOrDefault(b => b.User.ID == UserID)
            ?? throw new KeyNotFoundException($"User with ID {UserID} is not logged in.");

        // Verify refresh token is not previous refresh token
        if (IsEqual(
            Refresh["signature"]["signature"].GetString()!,
            Auth.PrevRef))
        {
            // Invalidate the tokens, unauthorized access attempt occurred
            Auth.Authorization = GenerateRandomString(64);
            Auth.Refresh = GenerateRandomString(64);
            Auth.PrevRef = GenerateRandomString(64);
            Context.SaveChanges();
            throw new AuthenticationException("Access denied.");
        }

        // Verify access token matches user
        if (!IsEqual(
            Access["signature"]["signature"].GetString()!,
            Auth.Authorization)
        ) throw new AuthenticationException("Access denied.");

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

        Context.SaveChanges();

        if (httpContext != null) AddHttpOnlyTokens(httpContext, AccessToken, RefreshToken);

        return (
            new Dictionary<string, string>
            {
                ["access"] = AccessToken,
                ["refresh"] = RefreshToken
            },
            UserID
        );
    }

    /// <summary>
    /// Returns true if strings are equal (in a cryptographically fixed time), false otherwise.
    /// </summary>
    /// <param name="String1"></param>
    /// <param name="String2"></param>
    /// <returns></returns>
    public static bool IsEqual(string? String1, string? String2)
    {
        if (String1 == null && String2 == null)
            return true;
        else if (String1 == null || String2 == null)
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(String1),
            Encoding.UTF8.GetBytes(String2));
    }

    /// <summary>
    /// Returns true if token is not expired, otherwise false
    /// </summary>
    /// <param name="ExpirationTime"></param>
    /// <returns></returns>
    public static bool IsExpired(DateTimeOffset ExpirationTime)
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
                iss = $"{ISSUER}:{ASP_PORT}",
                sub = UserID,
                exp = ExpiresAt,
                iat = IssuedAt
            });
    }

    /// <summary>
    /// Generates a token for use in MFA requests.
    /// </summary>
    /// <param name="UserID"></param>
    /// <returns></returns>
    public static string GenerateMFAPassCode(
        int UserID,
        KinnectionContext Context)
    {
        var CurrAuth = Context.Authentications
            .First(a => a.User.ID == UserID);

        long IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds();
        var PassCode = GenerateRandomString(6, true);

        CurrAuth.Reset = SignToken(JsonSerializer.Serialize(
            new
            {
                iss = $"{ISSUER}:{ASP_PORT}",
                sub = UserID,
                exp = ExpiresAt,
                iat = IssuedAt,
                aud = new List<string> { "/auth/mfa/" },
                psc = PassCode
            }));
        Context.SaveChanges();

        return PassCode;
    }

    /// <summary>
    /// Generates a token for use in password resetting
    /// </summary>
    /// <param name="Email"></param>
    /// <returns>Password Reset URL</returns>
    public static string GeneratePassResetURL(
        string Email)
    {
        using var Context = DatabaseManager.GetActiveContext();

        var CurrAuth = Context.Authentications
            .Include(a => a.User)
            .First(a => a.User.Email == Email);

        long IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds();

        var SignedToken = SignToken(JsonSerializer.Serialize(
            new
            {
                iss = $"{ISSUER}:{ASP_PORT}",
                sub = CurrAuth.User.ID,
                exp = ExpiresAt,
                iat = IssuedAt,
                aud = new List<string> { "/auth/pass/reset/" }
            }));

        CurrAuth.Reset = SignedToken;
        Context.SaveChanges();

        return $"{ISSUER}/reset/{Base64UrlEncoder.Encode(SignedToken)}";
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
                iss = $"{ISSUER}:{ASP_PORT}",
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
    public static string GenerateRandomString(int length = 24, bool NumberOnly = false)
    {
        char[] CharSet;
        if (NumberOnly)
            CharSet = ['1', '2', '3', '4', '5', '6', '7', '8', '9', '0'];
        else
            CharSet =
            [
                'A','B','C','D','E','F','G','H','I','J','K','L','M',
                'N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
                'a','b','c','d','e','f','g','h','i','j','k','l','m',
                'n','o','p','q','r','s','t','u','v','w','x','y','z',
                '0','1','2','3','4','5','6','7','8','9','#','@','$'
            ];

        return RandomNumberGenerator.GetString(CharSet, length);
    }

    /// <summary>
    /// Processes the authentication tokens from the cookies in the request
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns>The Access and Refresh authentication tokens</returns>
    /// <exception cref="AuthenticationException"></exception>
    private static (string Access, string Refresh) ProcessCookies(HttpContext httpContext)
    {
        string Access = string.Empty, Refresh = string.Empty;
        foreach (var Cookie in httpContext.Request.Cookies)
        {
            string CookieContent = Cookie.Value.Split("; ")[0];
            if ("Authorization" == Cookie.Key && CookieContent.StartsWith("Bearer "))
            {
                var CookieParts = CookieContent.Split(" ");
                if (CookieParts.Length != 2)
                    throw new AuthenticationException("Invalid authorization cookie format.");
                Access = CookieParts[1];
            }
            else if ("X-Refresh-Token" == Cookie.Key) Refresh = CookieContent;
        }

        if (string.IsNullOrWhiteSpace(Access) || string.IsNullOrWhiteSpace(Refresh))
            throw new AuthenticationException("Processing cookies failed.");

        return (Access, Refresh);
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

            var JsonSignature = new { signature = EncodedSignature };

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
        var Auth = Context.Authentications.FirstOrDefault(b => b.User.ID == UserID);

        string AccessToken = SignToken(GenerateAccessPayload(UserID));
        string RefreshToken = SignToken(GenerateRefreshPayload(UserID));
        string AccessHash = AccessToken.Split('.')[2];
        string RefreshHash = RefreshToken.Split('.')[2];

        if (ExistingUser == null)
            throw new KeyNotFoundException($"User {UserID} does not exist.");
        else if (Auth == null)
        {
            Auth = new Authentication
            {
                Created = DateTime.UtcNow,
                User = ExistingUser,
                Authorization = AccessHash,
                Refresh = RefreshHash,
                PrevRef = "",
                Reset = GenerateRandomString()
            };
            Context.Add(Auth);
        }
        else
        {
            Auth.Authorization = AccessHash;
            Auth.Refresh = RefreshHash;
            Auth.Reset = GenerateRandomString();
        }

        Context.SaveChanges();

        if (httpContext != null) AddHttpOnlyTokens(httpContext, AccessToken, RefreshToken);

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

    private static void ValidateTokenClaims(
        Dictionary<string, Dictionary<string, JsonElement>> ProcessedToken)
    {
        // Validate the header
        if (!ProcessedToken["header"].TryGetValue("alg", out var alg))
            throw new AuthenticationException("Authentication token is missing algorithm claim.");

        if (alg.GetString() != "RS256")
            throw new AuthenticationException("Authentication token algorithm is invalid.");

        if (!ProcessedToken["header"].TryGetValue("typ", out var jwt))
            throw new AuthenticationException("Authentication token is missing type claim.");

        if (jwt.GetString() != "JWT")
            throw new AuthenticationException("Authentication token type is invalid.");

        // Validate the payload
        if (!ProcessedToken["payload"].TryGetValue("exp", out var exp))
            throw new AuthenticationException("Authentication token is missing expiration claim.");

        if (IsExpired(DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64())))
            throw new AuthenticationException("Token has expired.");

        if (!ProcessedToken["payload"].ContainsKey("sub"))
            throw new AuthenticationException("Missing subject claim.");
    }

    /// <summary>
    /// Verifies that the JWT is untampered.
    /// </summary>
    /// <param name="Token"></param>
    /// <returns>Returns true if the signed JWT is untampered, false otherwise.</returns>
    public static bool VerifyToken(string Token)
    {
        try
        {
            string[] TokenParts = Token.Split('.');
            if (TokenParts.Length != 3)
                throw new Exception("Invalid token provided.");

            byte[] EncodedMessage = Encoding.UTF8.GetBytes($"{TokenParts[0]}.{TokenParts[1]}");
            byte[] DecodedSignature = Base64UrlEncoder.DecodeBytes(TokenParts[2]);

            using RSA rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(
                Convert.FromBase64String(KeyMaster.GetKeys().Public), out _);

            return rsa.VerifyData(
                EncodedMessage,
                DecodedSignature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }
        catch (Exception) { throw new AuthenticationException("Invalid token provided."); }
    }
}