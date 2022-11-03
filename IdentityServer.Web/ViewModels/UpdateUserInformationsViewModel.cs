using IdentityServer.Web.Enums;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace IdentityServer.Web.ViewModels
{
    public class UpdateUserInformationsViewModel
    {
        [Required(ErrorMessage = "Kullanici adi gereklidir")]
        [Display(Name = "Kullanici adi")]
        public string UserName { get; set; }

        [Display(Name = "Telefon no")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Sehir")]
        public string City { get; set; }

        public string Picture { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Dogum tarihi")]
        public DateTime BirthDate { get; set; }

        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Email adresi gereklidir")]
        [Display(Name = "Email adresi")]
        [EmailAddress(ErrorMessage = "Email adresiniz dogru formatta degil")]
        public string Email { get; set; }
    }
}
