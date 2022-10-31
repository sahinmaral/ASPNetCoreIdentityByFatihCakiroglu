namespace IdentityServer.Web.Models
{
    public class EmailServiceConfigurationModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool IsSSLEnabled { get; set; }
        public string Host { get; set; }
    }
}