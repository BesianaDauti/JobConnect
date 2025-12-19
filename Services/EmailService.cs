using JobConnect.Services;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendVerificationEmail(string toEmail, string token)
    {
        Console.WriteLine("📨 Email service called!");
        var host = _config["Smtp:Host"];
        var port = int.Parse(_config["Smtp:Port"]);
        var email = _config["Smtp:Email"];
        var password = _config["Smtp:Password"];

        var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(email, password),
            EnableSsl = true
        };

        string verifyLink = $"{_config["AppUrl"]}/Accounts/VerifyEmail?token={token}";

        var message = new MailMessage(email, toEmail)
        {
            Subject = "Verify your JobConnect Account",
            Body = $"Click the link to verify your email:\n\n{verifyLink}",
            IsBodyHtml = false
        };

        await client.SendMailAsync(message);
    }
    public async Task SendMailAsync(string toEmail, string subject, string body, string replyToEmail)
    {
        var host = _config["Smtp:Host"];
        var port = int.Parse(_config["Smtp:Port"]);
        var email = _config["Smtp:Email"];
        var password = _config["Smtp:Password"];

        var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(email, password),
            EnableSsl = true
        };

        var message = new MailMessage(email, toEmail)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.ReplyToList.Add(new MailAddress(replyToEmail));
        await client.SendMailAsync(message);
    }
    public async Task SendSimpleEmailAsync(string toEmail, string subject, string body)
    {
        var host = _config["Smtp:Host"];
        var port = int.Parse(_config["Smtp:Port"]);
        var email = _config["Smtp:Email"];
        var password = _config["Smtp:Password"];

        var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(email, password),
            EnableSsl = true
        };

        var message = new MailMessage(email, toEmail)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true   
        };

        await client.SendMailAsync(message);
    }

}
