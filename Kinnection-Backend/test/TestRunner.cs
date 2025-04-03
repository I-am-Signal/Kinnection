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
    /// If 'AssertAccess' is true, asserts Access token is valid.
    /// If 'AssertRefresh' is true, asserts Refresh token is valid.
    /// </summary>
    /// <param name="Headers"></param>
    /// <param name="AssertAccess"></param>
    /// <param name="AssertRefresh"></param>
    public static void CheckTokens(
        HttpResponseHeaders? Headers = null,
        Dictionary<string, string>? Tokens = null,
        bool AssertAccess = true,
        bool AssertRefresh = true)
    {
        string Access, Refresh;
        if (Headers != null)
        {
            // Account for the "Bearer XXXXXXX"
            Access = Headers.GetValues("Authorization").ElementAt(0).Split(" ")[1];
            Refresh = Headers.GetValues("X-Refresh-Token").ElementAt(0);
        }
        else
        {
            Access = Tokens!["access"];
            Refresh = Tokens!["refresh"];
        }

        // Ensure they are not empty
        Assert.That(string.IsNullOrEmpty(Access), Is.False);
        Assert.That(string.IsNullOrEmpty(Refresh), Is.False);

        bool IsAccessValid = Authenticator.VerifyToken(Access);
        bool IsRefreshValid = Authenticator.VerifyToken(Refresh);

        // Check asserts
        if (AssertAccess)
            Assert.That(IsAccessValid, Is.True);
        else
            Assert.That(IsAccessValid, Is.False);

        if (AssertRefresh)
            Assert.That(IsRefreshValid, Is.True);
        else
            Assert.That(IsRefreshValid, Is.False);
    }

    /// <summary>
    /// Provides the headers dictionary.
    /// </summary>
    /// <param name="Headers"></param>
    /// <returns>Dictionary with "Authorization" and "X-Refresh-Token" attributes</returns>
    public static Dictionary<string, string> GetHeaders(Dictionary<string, string>? Headers = null)
    {
        if (null == Headers)
        {
            Headers = new Dictionary<string, string>()
            {
                ["Authorization"] = $"Bearer {Access}",
                ["X-Refresh-Token"] = Refresh
            };
        }
        else
        {
            Headers["Authorization"] = $"Bearer {Access}";
            Headers["X-Refresh-Token"] = Refresh;
        }
        return Headers;
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
            string ASP_PORT = Environment.GetEnvironmentVariable("ASP_PORT")!;

            if (string.IsNullOrEmpty(ISSUER) || string.IsNullOrEmpty(ASP_PORT))
                throw new ApplicationException("Environment variables are null!");
            URI = $"{ISSUER}:{ASP_PORT}/";
        }
        return URI;
    }

    public static void SaveTokens(
        HttpResponseHeaders? Headers = null,
        Dictionary<string, string>? Tokens = null)
    {
        if (Headers != null)
        {
            // Account for the "Bearer XXXXXXX"
            Access = Headers.GetValues("Authorization").ElementAt(0).Split(" ")[1];
            Refresh = Headers.GetValues("X-Refresh-Token").ElementAt(0);
        }
        else if (Tokens != null)
        {
            Access = Tokens!["access"];
            Refresh = Tokens!["refresh"];
        }
        else
            throw new Exception("Provide either a token dictionary or the headers!");
    }

    /// <summary>
    /// Returns the JsonElement version of the parameter.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static JsonElement ToJsonElement<T>(T value)
    {
        return JsonSerializer.Deserialize<JsonElement>(
            JsonSerializer.Serialize(value)
        );
    }
}