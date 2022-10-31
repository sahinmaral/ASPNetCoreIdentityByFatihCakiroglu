using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Web.ViewModels
{
    public class UserSignUpViewModel
    {
        [Required(ErrorMessage ="Kullanici adi gereklidir")]
        [Display(Name ="Kullanici adi")]
        public string Username { get; set; }

        [Display(Name = "Telefon No")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email adresi gereklidir")]
        [Display(Name = "Email Adresi")]
        [EmailAddress(ErrorMessage = "Email adresiniz dogru formatta degil")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Sifre gereklidir")]
        [Display(Name = "Sifre")]
        [MinLength(4, ErrorMessage = "Sifreniz en az 4 karakterli olmak zorundadir")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
