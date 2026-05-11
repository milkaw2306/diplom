using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Diplom_zxc.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailService(
            string smtpServer = "smtp.gmail.com",
            int smtpPort = 587,
            string username = "your-email@gmail.com",
            string password = "your-password")
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUsername = username;
            _smtpPassword = password;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                using var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_smtpUsername, "Diplom_zxc"),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
            }
            catch (Exception)
            {
                // В демо-режиме просто игнорируем ошибки отправки
                await Task.CompletedTask;
            }
        }
    }
}