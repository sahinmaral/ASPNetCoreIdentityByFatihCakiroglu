using IdentityServer.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace IdentityServer.Web.TagHelpers
{
    [HtmlTargetElement("td", Attributes = "user-roles")]

    public class UserRolesTagHelper:TagHelper
    {
        public UserManager<AppUser> UserManager { get; set; }

        [HtmlAttributeName("user-roles")]
        public string UserId { get; set; }

        public UserRolesTagHelper(UserManager<AppUser> userManager)
        {
            UserManager = userManager;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            AppUser user = await UserManager.FindByIdAsync(UserId);

            IList<string> roles = await UserManager.GetRolesAsync(user);

            string htmlContent = string.Empty;

            roles.ToList().ForEach(x =>
            {
                htmlContent += $"<span class='badge bg-primary'>{x}</span>";
            });

            output.Content.SetHtmlContent(htmlContent);
        }
    }
}
