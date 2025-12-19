namespace JobConnect.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmail(string email, string token);
        Task SendMailAsync(string toEmail, string subject, string body, string replyToEmail);
        Task SendSimpleEmailAsync(string toEmail, string subject, string body);
    }
}
