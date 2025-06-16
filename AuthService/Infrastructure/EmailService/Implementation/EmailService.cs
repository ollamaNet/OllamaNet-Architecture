using AuthService.Infrastructure.EmailService.Interfaces;
using AuthService.Infrastructure.EmailService.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.EmailService.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var email = new MimeMessage();
                
                email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                email.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, 
                    _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                
                if (!string.IsNullOrEmpty(_emailSettings.Username) && !string.IsNullOrEmpty(_emailSettings.Password))
                {
                    await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                }
                
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }

        public async Task SendPasswordResetEmailAsync(string to, string username, string resetLink)
        {
            string subject = "Reset Your Password - OllamaNet";
            
            string body = $@"<html>
            <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
                    <h2 style='color: #2c3e50;'>Password Reset Request</h2>
                    <p>Hello {username},</p>
                    <p>We received a request to reset your password for your OllamaNet account. To complete the password reset process, please click on the button below:</p>
                    <p style='text-align: center;'>
                        <a href='{resetLink}' style='display: inline-block; padding: 10px 20px; background-color: #3498db; color: white; text-decoration: none; border-radius: 4px;'>Reset Password</a>
                    </p>
                    <p>If you did not request a password reset, please ignore this email or contact support if you have concerns.</p>
                    <p>This link will expire in 24 hours.</p>
                    <p>Thank you,<br>The OllamaNet Team</p>
                </div>
            </body>
            </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendRegistrationSuccessEmailAsync(string to, string username)
        {
            string subject = "Welcome to OllamaNet - Registration Successful";
            
            string body = $@"<html>
            <body style='font-family: Arial, sans-serif; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
                    <h2 style='color: #2c3e50;'>Welcome to OllamaNet!</h2>
                    <p>Hello {username},</p>
                    <p>Thank you for registering with OllamaNet. Your account has been successfully created and is ready to use.</p>
                    <p>With your OllamaNet account, you can:</p>
                    <ul>
                        <li>Access AI models for various tasks</li>
                        <li>Create and manage conversations</li>
                        <li>Organize your content in folders</li>
                        <li>And much more!</li>
                    </ul>
                    <p style='text-align: center;'>
                        <a href='https://localhost:7006/login' style='display: inline-block; padding: 10px 20px; background-color: #3498db; color: white; text-decoration: none; border-radius: 4px;'>Login to Your Account</a>
                    </p>
                    <p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
                    <p>Thank you for joining us!</p>
                    <p>Best regards,<br>The OllamaNet Team</p>
                </div>
            </body>
            </html>";

            await SendEmailAsync(to, subject, body);
        }
    }
}
