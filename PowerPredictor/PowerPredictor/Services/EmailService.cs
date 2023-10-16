using Microsoft.Extensions.Options;
using PowerPredictor.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace PowerPredictor.Services
{
    public class EmailServiceConfiguration
    {
        public string MailServer { get; set; } = null!;
        public int MailPort { get; set; }
        public string SenderMail { get; set; } = null!;
        public string SenderPassword { get; set; } = null!;
    }

    public class EmailService : IEmailService
    {
        private readonly EmailServiceConfiguration options;

        private SmtpClient smtpClient;

        public EmailService(IOptions<EmailServiceConfiguration> options)
        {
            this.options = options.Value;

            smtpClient = new SmtpClient(this.options.MailServer)
            {
                Port = this.options.MailPort,
                Credentials = new NetworkCredential(this.options.SenderMail, this.options.SenderPassword),
                EnableSsl = true,
            };
        }

        public void SendEmail(string email, string subject, string message)
        {

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(options.SenderMail);
            mailMessage.To.Add(new MailAddress(email));
            mailMessage.Subject = subject;
            mailMessage.Body = message;

            smtpClient.Send(mailMessage);
        }

        public void SendResetPasswordEmail(string email, string callbackUrl)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(options.SenderMail);
            mailMessage.To.Add(new MailAddress(email));
            mailMessage.Subject = "Reset your password";
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = "Click <a href = \"" + callbackUrl + "\" > here </ a > to change your password in PowerPredictor";

            smtpClient.Send(mailMessage);
        }

        public void SendVerifyAccountEmail(string email, string callbackUrl)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(options.SenderMail);
            mailMessage.To.Add(new MailAddress(email));
            mailMessage.Subject = "Activate your PowerPredictor account";
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = "Click <a href = \"" + callbackUrl + "\" > here </a > to activate your account in PowerPredictor";

            smtpClient.Send(mailMessage);
        }
    }
}
