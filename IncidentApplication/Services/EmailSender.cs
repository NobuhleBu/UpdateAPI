using IncidentApplication.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace IncidentApplication.Services
{
    public enum WhichEmail
    {
        ConfirmEmail,
        ResetPassword
    }

    public class SendEmailTemplateData
    {
        [JsonProperty("verifyLink")]
        public string VeriyLink { get; set; }
    }

    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public Task ExecuteSendEmail(ApplicationUser user, string email, string url, WhichEmail whichEmail)
        {
            var client = new SendGridClient(_config["SendGrid:Key"]);
            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress(_config["SendGrid:SenderEmail"], _config["SendGrid:SenderName"]));
            msg.AddTo(new EmailAddress(email, user.FirstName + " " + user.LastName));

            switch(whichEmail)
            {
                case WhichEmail.ConfirmEmail:
                    msg.SetTemplateId(_config["SendGrid:ConfirmEmailTransID"]);
                    break;
                case WhichEmail.ResetPassword:
                    msg.SetTemplateId(_config["SendGrid:ConfirmEmailTransID"]);
                    break;
            }

            var dynamicTemplateData = new SendEmailTemplateData
            {
                VeriyLink = url
            };

            msg.SetTemplateData(dynamicTemplateData);

            return client.SendEmailAsync(msg);
        }

        public Task SendConfirmEmailAsync(ApplicationUser user, string email, string url)
        {
            return ExecuteSendEmail(user, email, url, WhichEmail.ConfirmEmail);
        }

        public Task SendRestPasswordEmailAsync(ApplicationUser user, string email, string url)
        {
            return ExecuteSendEmail(user, email, url, WhichEmail.ResetPassword);
        }
    }
}
