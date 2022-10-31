using IdentityServer.Web.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;

namespace IdentityServer.Web.Helpers
{
    public static class PasswordResetHelper
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

        public static void PasswordResetSendEmail(string link,string email)
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

            mail.Subject = "www.blarun.com::Sifre sifirlama";
            mail.Body = "<h2>Sifrenizi yenilemek icin lutfen asagidaki linke tiklayiniz </h2><hr/>";
            mail.Body += $"<a href='{link}'>Sifre yenileme linki</a>";
            mail.IsBodyHtml = true;

            smtpClient.Send(mail);

        }
    }
}
