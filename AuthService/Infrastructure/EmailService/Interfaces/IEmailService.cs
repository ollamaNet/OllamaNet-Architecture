using System.Threading.Tasks;

namespace AuthService.Infrastructure.EmailService.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendPasswordResetEmailAsync(string to, string username, string resetLink);
        Task SendRegistrationSuccessEmailAsync(string to, string username);
    }
}
