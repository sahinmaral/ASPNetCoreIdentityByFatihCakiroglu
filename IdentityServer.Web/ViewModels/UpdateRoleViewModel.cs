using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace IdentityServer.Web.ViewModels
{
    public class UpdateRoleViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Rol ismi")]
        [Required(ErrorMessage = "Rol ismi gereklidir")]
        public string Name { get; set; }
    }
}
