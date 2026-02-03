using System.Net;
using System.Net.Mail;

namespace ThesisBackend.Services.AuthServices
{
    public class EmailService(string smtpServer, int smtpPort, string senderEmail, string senderName, string senderPassword)
    {
        private readonly string _smtpServer = smtpServer;
        private readonly int _smtpPort = smtpPort;
        private readonly string _senderEmail = senderEmail;
        private readonly string _senderName = senderName;
        private readonly string _senderPassword = senderPassword;

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_senderEmail, _senderPassword),
                EnableSsl = true
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(recipientEmail);
            await client.SendMailAsync(mailMessage);
        }
    }
}
