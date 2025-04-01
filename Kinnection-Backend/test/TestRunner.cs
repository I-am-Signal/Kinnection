using System.Net.Http.Headers;
using System.Text.Json;
using Kinnection;
using NUnit.Framework;

namespace test;

public static class TestRunner
{
    public static string Access = string.Empty;
    public static readonly Keys EncryptionKeys = KeyMaster.GetKeys();
    public static string Public = string.Empty;
    public static string Refresh = string.Empty;
    private static string URI = string.Empty;

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

        bool IsAccessValid = Authenticator.VerifyToken(Access);
        bool IsRefreshValid = Authenticator.VerifyToken(Refresh);

        // Check asserts
        if (AssertAccess)
            Assert.That(IsAccessValid, Is.True);
        else
            Assert.That(IsAccessValid, Is.True);

        if (AssertRefresh)
            Assert.That(IsRefreshValid, Is.True);
        else
            Assert.That(IsRefreshValid, Is.True);
    }

    public static void SaveTokens(HttpResponseHeaders Headers)
    {
        // Account for the "Bearer XXXXXXX"
        Access = Headers.GetValues("Authorization").ElementAt(0).Split(" ")[1];
        Refresh = Headers.GetValues("X-Refresh-Token").ElementAt(0);
    }
}