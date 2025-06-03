using System.Net.Http.Headers;
using System.Text.Json;
using Kinnection;
using NUnit.Framework;

namespace test;

public static class TestRunner
{
    private static string Access = string.Empty;
    public static readonly Keys EncryptionKeys = KeyMaster.GetKeys();
    private static string Refresh = string.Empty;
    private static string URI = string.Empty;

    /// <summary>
    /// Verifies token validity. 
    /// If 'AssertValidAccess' is true, asserts Access token is valid.
    /// If 'AssertValidRefresh' is true, asserts Refresh token is valid.
    /// </summary>
    /// <param name="Headers"></param>
    /// <param name="AssertValidAccess"></param>
    /// <param name="AssertValidRefresh"></param>
    public static void CheckTokens(
        HttpResponseHeaders? Headers = null,
        Dictionary<string, string>? Tokens = null,
        bool AssertValidAccess = true,
        bool AssertValidRefresh = true)
    {
        string AccessToken = string.Empty, RefreshToken = string.Empty;
        if (Headers != null)
        {
            var Cookies = Headers.GetValues("Set-Cookie");
            foreach (var Cookie in Cookies)
            {
                var ParsedCookie = Cookie.Split("=");
                string CookieName = ParsedCookie[0];
                string CookieContent = ParsedCookie[1].Split("; ")[0];
                if ("Authorization" == CookieName) AccessToken = CookieContent.Split("%20")[1];
                else if ("X-Refresh-Token" == CookieName) RefreshToken = CookieContent;
            }
        }
        else
        {
            AccessToken = Tokens!["access"];
            RefreshToken = Tokens!["refresh"];
        }

        // Ensure they are not empty
        Assert.That(string.IsNullOrEmpty(AccessToken), Is.False);
        Assert.That(string.IsNullOrEmpty(RefreshToken), Is.False);

        bool IsAccessValid = Authenticator.VerifyToken(AccessToken);
        bool IsRefreshValid = Authenticator.VerifyToken(RefreshToken);

        // Check asserts
        if (AssertValidAccess)
        {
            Assert.That(IsAccessValid, Is.True);
            Access = AccessToken;
        }
        else Assert.That(IsAccessValid, Is.False);

        if (AssertValidRefresh)
        {
            Assert.That(IsRefreshValid, Is.True);
            Refresh = RefreshToken;
        }
        else Assert.That(IsRefreshValid, Is.False);
    }

    /// <summary>
    /// Provides the headers dictionary.
    /// </summary>
    /// <param name="Headers"></param>
    /// <returns>Dictionary with "Authorization" and "X-Refresh-Token" attributes</returns>
    public static Dictionary<string, string> GetHeaders(bool Decomposed = false)
    {
        if (!Decomposed)
            return new Dictionary<string, string>
            {
                ["Cookie"] = $"Authorization=Bearer%20{Access}; X-Refresh-Token={Refresh}"
            };
        else
            return new Dictionary<string, string>
            {
                ["Authorization"] = Access,
                ["X-Refresh-Token"] = Refresh
            };
    }

    /// <summary>
    /// Provides the Tokens.
    /// </summary>
    /// <returns>Dictionary with "access" and "refresh" tokens</returns>
    public static Dictionary<string, string> GetTokens()
    {
        return new Dictionary<string, string>()
        {
            ["access"] = Access,
            ["refresh"] = Refresh
        };
    }

    /// <summary>
    /// Returns the URI of the site.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ApplicationException"></exception>
    public static string GetURI()
    {
        if (URI == string.Empty)
        {
            string ISSUER = Environment.GetEnvironmentVariable("ISSUER")!;
            string ASP_PORT = Environment.GetEnvironmentVariable("ASP_EXTERNAL_PORT")!;

            if (string.IsNullOrEmpty(ISSUER) || string.IsNullOrEmpty(ASP_PORT))
                throw new ApplicationException("Environment variables are null!");
            URI = $"{ISSUER}:{ASP_PORT}/";
        }
        return URI;
    }

    /// <summary>
    /// Asserts that all values of Object are in and equivalent to that of Expected
    /// </summary>
    /// <param name="Object"></param>
    /// <param name="Expected"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void EvaluateJsonElementObject(
        JsonElement Object, JsonElement Expected)
    {
        switch (Object.ValueKind)
        {
            case JsonValueKind.String:
                Assert.That(
                    Object.GetString(),
                    Is.EqualTo(Expected.GetString()),
                    $"Object: \"{Object.GetString()}\", Expected: \"{Expected.GetString()}\"");
                break;
            case JsonValueKind.Number:
                Assert.That(
                    Object.GetInt32(),
                    Is.EqualTo(Expected.GetInt32()),
                    $"Object: \"{Object.GetInt32()}\", Expected: \"{Expected.GetInt32()}\"");
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                Assert.That(
                    Object.GetBoolean(),
                    Is.EqualTo(Expected.GetBoolean()),
                    $"Object: \"{Object.GetBoolean()}\", Expected: \"{Expected.GetBoolean()}\"");
                break;
            case JsonValueKind.Null:
                Assert.That(
                    Expected.ValueKind,
                    Is.EqualTo(JsonValueKind.Null),
                    $"Object: \"{Object.ValueKind}\", Expected: \"{Expected.ValueKind}\"");
                break;
            case JsonValueKind.Array:
                var ObjArr = Object.EnumerateArray();
                var ExpArr = Expected.EnumerateArray();

                Assert.That(
                    ObjArr.Count(),
                    Is.EqualTo(ExpArr.Count()),
                    $"Object: \"{ObjArr.Count()}\", Expected: \"{ExpArr.Count()}\"");

                var ObjEnum = ObjArr.GetEnumerator();
                var ExpEnum = ExpArr.GetEnumerator();

                while (ObjEnum.MoveNext() && ExpEnum.MoveNext())
                    EvaluateJsonElementObject(ObjEnum.Current, ExpEnum.Current);
                break;
            case JsonValueKind.Object:
                Console.WriteLine($"Object: {Object}");
                Console.WriteLine($"Expected: {Expected}");
                foreach (var property in Object.EnumerateObject())
                {
                    if ("id" == property.Name && JsonValueKind.Null == Expected.GetProperty(property.Name).ValueKind)
                    {
                        Console.WriteLine($"ID Found: {Object.GetProperty("id")}");
                        Assert.That(
                            property.Value.ValueKind == JsonValueKind.Null ||
                             property.Value.ValueKind == JsonValueKind.Number,
                            Is.True,
                            "Expected property \"ID\" JsonValueKind to be Null or Number"
                        );
                    }
                    else
                    {
                        Console.WriteLine($"Object: \"{property.Name}\":\"{property.Value}\"");
                        Console.WriteLine($"Expected-Value: \"{Expected.GetProperty(property.Name)}\"");
                        EvaluateJsonElementObject(
                            property.Value, Expected.GetProperty(property.Name));
                    }
                }
                break;
            default:
                throw new ArgumentException("ValueKind of JsonElement is not valid");
        }
    }
}