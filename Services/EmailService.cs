using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Diplom_zxc.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailService(string smtpServer = "smtp.gmail.com", int smtpPort = 587,
                           string smtpUsername = "your-email@gmail.com", string smtpPassword = "your-app-password")
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUsername = smtpUsername;
            _smtpPassword = smtpPassword;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Diplom_zxc", _smtpUsername));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}