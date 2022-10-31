using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Web.ViewModels
{
    public class UserLoginViewModel
    {
        [Required(ErrorMessage = "Email adresi gereklidir")]
        [Display(Name = "Email Adresi")]
        [EmailAddress(ErrorMessage = "Email adresiniz dogru formatta degil")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Sifre gereklidir")]
        [Display(Name = "Sifre")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Beni Hatirla")]
        public bool RememberMe { get; set; }
    }
}
