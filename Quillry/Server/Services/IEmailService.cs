
namespace Quillry.Server.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string fromEmail, string toEmail, string subject, string message);
    }
}