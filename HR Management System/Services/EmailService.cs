using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace HR_Management_System.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using (var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"]))
            {
                smtpClient.Port = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                smtpClient.Credentials = new NetworkCredential(
                    _configuration["EmailSettings:SenderEmail"],
                    _configuration["EmailSettings:SenderPassword"]
                );
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_configuration["EmailSettings:SenderEmail"]);
                    mailMessage.To.Add(toEmail);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
        }
    }
}
