using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Web.ViewModels
{
    public class UpdatePasswordViewModel
    {
        [Display(Name = "Eski sifre")]
        [Required(ErrorMessage = "Eski sifreniz gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4,ErrorMessage = "Sifreniz en az 4 karakterli olmak zorundadir")]
        public string PasswordOld { get; set; }

        [Display(Name = "Yeni sifre")]
        [Required(ErrorMessage = "Yeni sifreniz gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Yeni sifreniz en az 4 karakterli olmak zorundadir")]
        public string PasswordNew { get; set; }

        [Display(Name = "Yeni sifre onay")]
        [Required(ErrorMessage = "Yeni sifre onayi gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Yeni sifre onayiniz en az 4 karakterli olmak zorundadir")]
        [Compare(nameof(PasswordNew),ErrorMessage = "Yeni sifreniz ve onay sifreniz ayni olmak zorundadir")]
        public string PasswordConfirm { get; set; }
    }
}
