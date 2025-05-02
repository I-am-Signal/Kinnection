using SendGrid;
using SendGrid.Helpers.Mail;

namespace Kinnection;

public class JustGonnaSendIt
{
    private static SendGridClient? SGClient;
    private static EmailAddress? From;

    public static async Task<Response> SendEmail(
        EmailAddress Address,
        string Subject,
        string PlainTextContent,
        string HTMLContent)
    {
        if (SGClient == null || From == null) // Ensure email system is ready for use
        {
            // Get env vars
            string APIKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")!;
            string Email = Environment.GetEnvironmentVariable("FROM_EMAIL")!;
            string Name = Environment.GetEnvironmentVariable("FROM_NAME")!;

            if (string.IsNullOrWhiteSpace(APIKey) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Name))
            { throw new Exception("SendGrid API environment variables not found!"); }

            // Set vars
            SGClient = new SendGridClient(APIKey);
            From = new EmailAddress(Email, Name);
        }

        return await SGClient!.SendEmailAsync(
            MailHelper.CreateSingleEmail(
                from: From,
                to: Address,
                subject: Subject,
                plainTextContent: PlainTextContent,
                htmlContent: HTMLContent
            )
        );
    }
}