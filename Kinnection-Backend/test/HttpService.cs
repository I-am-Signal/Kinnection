using System.Text;
namespace test;

public static class HttpService
{
    private static readonly HttpClient _client = new HttpClient();

    private static string AppendArguments(
        string URI,
        Dictionary<string, string>? Arguments)
    {
        if (Arguments == null)
            return URI;

        StringBuilder URL = new StringBuilder(URI + "?");
        for (int i = 0; i < Arguments.Count; i++)
        {
            string Argument = Arguments.ElementAt(i).Key;
            string Value = Arguments.ElementAt(i).Value;
            URL.Append($"{Argument}={Value}");
        }
        return URL.ToString();
    }

    private static string JSONStringifyDictionary(
        Dictionary<string, string> Content)
    {
        StringBuilder Output = new StringBuilder();
        for (int i = 0; i < Content.Count; i++)
        {
            string Key = Content.ElementAt(i).Key;
            string Value = Content.ElementAt(i).Value;
            Output.Append($"\"{Key}\": \"{Value}\"");
            if (i != Content.Count - 1)
                Output.Append(", ");
        }
        return '{' + Output.ToString() + '}';
    }

    public static async Task<HttpResponseMessage> GetAsync(
        string URI,
        Dictionary<string, string>? Arguments = null)
    {
        return await _client.GetAsync(AppendArguments(URI, Arguments));
    }

    public static async Task<HttpResponseMessage> PostAsync(
        string URI,
        Dictionary<string, string> Data,
        string ContentType = "application/json",
        Dictionary<string, string>? Arguments = null)
    {
        return await _client.PostAsync(
            AppendArguments(URI, Arguments),
            new StringContent(
                JSONStringifyDictionary(Data),
                Encoding.UTF8,
                ContentType)
        );
    }

    public static async Task<HttpResponseMessage> PutAsync(
        string URI,
        Dictionary<string, string> Data,
        string ContentType = "application/json",
        Dictionary<string, string>? Arguments = null)
    {
        return await _client.PutAsync(
            AppendArguments(URI, Arguments),
            new StringContent(
                JSONStringifyDictionary(Data),
                Encoding.UTF8,
                ContentType)
        );
    }

    public static async Task<HttpResponseMessage> DeleteAsync(
        string URI,
        Dictionary<string, string>? Arguments = null)
    {
        return await _client.DeleteAsync(
            AppendArguments(URI, Arguments)
        );
    }
}