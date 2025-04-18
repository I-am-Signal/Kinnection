using System.Text;
using System.Text.Json;

namespace test;

public static class HttpService
{
    private static readonly HttpClient _client = new HttpClient();

    private static HttpRequestMessage AddHeaders(
        HttpRequestMessage Request,
        Dictionary<string, string>? Headers)
    {
        if (Headers != null)
        {
            foreach (var header in Headers)
                Request.Headers.Add(header.Key, header.Value);
        }

        return Request;
    }

    private static string AppendArguments(
        string URI,
        Dictionary<string, string>? Arguments)
    {
        if (Arguments == null)
            return URI;

        StringBuilder URL = new StringBuilder();
        URL.Append(URI + "?");
        for (int i = 0; i < Arguments.Count; i++)
        {
            string Argument = Arguments.ElementAt(i).Key;
            string Value = Arguments.ElementAt(i).Value;
            URL.Append($"{Argument}={Value}");
            if (Arguments.Count > 1 && i < Arguments.Count - 1)
                URL.Append('&');
        }
        return URL.ToString();
    }

    public static async Task<HttpResponseMessage> GetAsync(
        string URI,
        string Parameter = "",
        Dictionary<string, string>? Arguments = null,
        Dictionary<string, string>? Headers = null)
    {
        var Request = new HttpRequestMessage(
            HttpMethod.Get,
            AppendArguments(URI + Parameter, Arguments));

        AddHeaders(Request, Headers);
        return await _client.SendAsync(Request);
    }

    public static async Task<HttpResponseMessage> PostAsync(
        string URI,
        Dictionary<string, JsonElement> Content,
        string ContentType = "application/json",
        string Parameter = "",
        Dictionary<string, string>? Arguments = null,
        Dictionary<string, string>? Headers = null)
    {
        var Request = new HttpRequestMessage(
            HttpMethod.Post,
            AppendArguments(URI + Parameter, Arguments))
        {
            Content = new StringContent(JsonSerializer.Serialize(Content), Encoding.UTF8, ContentType)
        };
        AddHeaders(Request, Headers);
        return await _client.SendAsync(Request);
    }

    public static async Task<HttpResponseMessage> PutAsync(
        string URI,
        Dictionary<string, JsonElement> Content,
        string ContentType = "application/json",
        string Parameter = "",
        Dictionary<string, string>? Arguments = null,
        Dictionary<string, string>? Headers = null)
    {
        var Request = new HttpRequestMessage(
            HttpMethod.Put,
            AppendArguments(URI + Parameter, Arguments))
        {
            Content = new StringContent(JsonSerializer.Serialize(Content), Encoding.UTF8, ContentType)
        };
        AddHeaders(Request, Headers);
        return await _client.SendAsync(Request);
    }

    public static async Task<HttpResponseMessage> DeleteAsync(
        string URI,
        string Parameter = "",
        Dictionary<string, string>? Arguments = null,
        Dictionary<string, string>? Headers = null)
    {
        var Request = new HttpRequestMessage(HttpMethod.Delete, AppendArguments(URI + Parameter, Arguments));
        AddHeaders(Request, Headers);
        return await _client.SendAsync(Request);
    }
}
