using IdentityServer.Web.Enums;

namespace IdentityServer.Web.ViewModels
{
    public class UserProfileViewModel
    {
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string Picture { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender Gender { get; set; }
    }
}
