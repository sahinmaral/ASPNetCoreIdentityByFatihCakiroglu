using IdentityServer.Web.Models;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;

namespace IdentityServer.Web.Helpers
{
    public static class EmailConfirmationHelper
    {
        private static EmailServiceConfigurationModel AssignEmailConfigurationAndReturn()
        {
            using (StreamReader r = new StreamReader(Directory.GetCurrentDirectory() + "/Constants/mailServiceConfiguration.json"))
            {
                string json = r.ReadToEnd();
                EmailServiceConfigurationModel config = JsonConvert.DeserializeObject<EmailServiceConfigurationModel>(json);
                return config;
            }
        }

        public static void EmailConfirmationSendEmail(string link, string email)
        {
            EmailServiceConfigurationModel config = AssignEmailConfigurationAndReturn();

            MailMessage mail = new MailMessage();

            var smtpClient = new SmtpClient(config.Host)
            {
                Port = config.Port,
                Credentials = new NetworkCredential(config.Email, config.Password),
                EnableSsl = config.IsSSLEnabled,
            };

            mail.From = new MailAddress(config.Email);
            mail.To.Add(email);

            mail.Subject = "www.blarun.com::Email dogrulama";
            mail.Body = "<h2>Hesabinizi dogrulamak icin lutfen asagidaki linke tiklayiniz </h2><hr/>";
            mail.Body += $"<a href='{link}'>Hesap dogrulama linki</a>";
            mail.IsBodyHtml = true;

            smtpClient.Send(mail);

        }
    }
}
