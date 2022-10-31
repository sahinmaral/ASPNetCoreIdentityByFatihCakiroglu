using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace IdentityServer.Web.ViewModels
{
    public class UserResetPasswordViewModel
    {
        
        [Required(ErrorMessage = "Email adresi gereklidir")]
        [Display(Name = "Email Adresi")]
        [EmailAddress(ErrorMessage = "Email adresiniz dogru formatta degil")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Sifre gereklidir")]
        [Display(Name = "Yeni Sifre")]
        [DataType(DataType.Password)]
        public string PasswordNew { get; set; }
    }
}
