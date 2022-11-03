using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace IdentityServer.Web.Claims
{
    public class PremiumExchangeRequirement:IAuthorizationRequirement
    {
    }

    public class PremiumExchangeHandler : AuthorizationHandler<PremiumExchangeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PremiumExchangeRequirement requirement)
        {
            var expireClaim = context.User.Claims.FirstOrDefault(x => x.Type == "PremiumExchangeExpireDate");
            if(expireClaim != null)
            {
                DateTime d1 = Convert.ToDateTime(expireClaim.Value);
                DateTime d2 = DateTime.Now;

                DateTime d1Converted = new DateTime(d1.Year, d1.Month, d1.Day);
                DateTime d2Converted = new DateTime(d2.Year, d2.Month, d2.Day);

                int result = DateTime.Compare(d1Converted,d2Converted);

                if (result > 0)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }

            return Task.CompletedTask;
        }
    }
}
