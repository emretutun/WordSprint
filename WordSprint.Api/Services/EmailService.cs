using System.Net;
using System.Net.Mail;

namespace WordSprint.Api.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var smtp = _config.GetSection("Smtp");

        var host = smtp["Host"]!;
        var port = int.Parse(smtp["Port"]!);
        var user = smtp["User"]!;
        var pass = smtp["Password"]!;
        var fromName = smtp["FromName"] ?? "WordSprint";
        var fromEmail = smtp["FromEmail"] ?? user;

        using var message = new MailMessage();
        message.From = new MailAddress(fromEmail, fromName);
        message.To.Add(toEmail);
        message.Subject = subject;
        message.Body = htmlBody;
        message.IsBodyHtml = true;

        using var client = new SmtpClient(host, port);
        client.Credentials = new NetworkCredential(user, pass);

        // Gmail 587 için STARTTLS
        client.EnableSsl = true;

        await client.SendMailAsync(message);
    }
}
