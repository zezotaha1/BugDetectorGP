using SendGrid.Helpers.Mail;
using SendGrid;
using SendGrid.Helpers.Mail.Model;

public class SendGridEmailSender
{
    private static string key = "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";

    public static async Task<string> SendEmail(string email, string subject, string plainTextContent)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Missing SendGrid API key. Please set it before using SendGrid.");
        }

        var client = new SendGridClient(key);

        var from = new EmailAddress("bugdetector8@gmail.com", "BugDetector"); // Replace with your sender email and name
        var to = new EmailAddress(email);

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, null);

        try
        {
            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                return "Email sent successfully.";
            }
            else
            {
                return $"Failed to send Email: Status code {response.StatusCode} - {response.Body.ReadAsStringAsync().Result}";
            }
        }
        catch (Exception ex)
        {
            return $"Failed to send Email: {ex.Message}";
        }
    }
}
