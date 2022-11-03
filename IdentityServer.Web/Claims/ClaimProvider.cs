using IdentityServer.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Security.Principal;

namespace IdentityServer.Web.ClaimProvider
{
    public class ClaimProvider : IClaimsTransformation
    {
        private readonly UserManager<AppUser> _userManager;
        public ClaimProvider(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal != null && principal.Identity.IsAuthenticated)
            {
                ClaimsIdentity identity = principal.Identity as ClaimsIdentity;
                AppUser user = await _userManager.FindByNameAsync(identity.Name);
                if (user != null)
                {

                    AddCityClaim(principal, identity, user);
                    AddViolenceClaim(principal, identity, user);


                }
            }

            return principal;
        }

        private void AddViolenceClaim(ClaimsPrincipal principal, ClaimsIdentity identity, AppUser user)
        {
            if (!principal.HasClaim(x => x.Type == "violence"))
            {
                int age = DateTime.Now.Year - user.BirthDate.Year;

                string violenceStatus = (age > 18).ToString();

                Claim violenceClaim = new Claim("violence", violenceStatus, ClaimValueTypes.String, "Internal");
                identity.AddClaim(violenceClaim);

            }
        }

        private void AddCityClaim(ClaimsPrincipal principal, ClaimsIdentity identity, AppUser user)
        {
            if (!principal.HasClaim(x => x.Type == "city"))
            {
                Claim cityClaim = new Claim("city", user.City, ClaimValueTypes.String, "Internal");
                identity.AddClaim(cityClaim);
            }
        }
    }
}
