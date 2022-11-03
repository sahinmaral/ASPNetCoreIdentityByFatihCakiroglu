

using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Web.ViewModels
{
    public class CreateRoleViewModel
    {
        [Display(Name="Rol ismi")]
        [Required(ErrorMessage="Rol ismi gereklidir")]
        public string Name { get; set; }
    }
}
