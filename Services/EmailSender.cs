using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace WebsiteBanHang.Services
{
    // Sử dụng interface từ Identity
    public class EmailSender : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    emailSettings["SenderName"],
                    emailSettings["SenderEmail"]
                ));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlMessage
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    emailSettings["SmtpServer"],
                    int.Parse(emailSettings["SmtpPort"]!),
                    SecureSocketOptions.StartTls
                );

                await client.AuthenticateAsync(
                    emailSettings["Username"],
                    emailSettings["Password"]
                );

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email to {email}: {ex.Message}");
                throw;
            }
        }
    }
}