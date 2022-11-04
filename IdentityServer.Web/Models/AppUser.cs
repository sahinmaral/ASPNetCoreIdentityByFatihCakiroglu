using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Web.Models
{
    public class AppUser:IdentityUser
    {
        public string City { get; set; }
        public string Picture { get; set; }
        public DateTime BirthDate { get; set; }
        public int Gender { get; set; }

        public AppUser()
        {
            City = "";
            Picture = "";
        }
    }
}
