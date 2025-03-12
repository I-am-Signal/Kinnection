using System.Text;

namespace test;

public static class TestRunner
{
    public static string Access = string.Empty;
    public static string Refresh = string.Empty;
    private static string URI = string.Empty;

    public static string GetURI()
    {
        if (URI == string.Empty){
            string? ISSUER = Environment.GetEnvironmentVariable("ISSUER");
            string? ASP_PORT = Environment.GetEnvironmentVariable("ASP_PORT"); 
            
            if (ISSUER == null || ASP_PORT == null)
                throw new Exception("Environment variables are null");    
            URI = $"{ISSUER}:{ASP_PORT}/";
        }
        return URI;
    }
}