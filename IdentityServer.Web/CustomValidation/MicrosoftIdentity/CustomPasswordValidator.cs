using IdentityServer.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Web.CustomValidation.MicrosoftIdentity
{
    public class CustomPasswordValidator : IPasswordValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            List<IdentityError> errors = new List<IdentityError>();

            if (password.ToLower().Contains(user.UserName.ToLower()))
            {
                if (!user.Email.Contains(user.UserName))
                {
                    errors.Add(new IdentityError()
                    {
                        Code = "PasswordContainsUsername",
                        Description = "Sifre , kullanici adi iceremez"
                    });
                }
                    
            }

            if (password.ToLower().Contains(user.Email.ToLower()))
            {
                errors.Add(new IdentityError()
                {
                    Code = "PasswordContainsEmail",
                    Description = "Sifre , email adresi iceremez"
                });
            }

            if (password.Contains("1234"))
            {
                errors.Add(new IdentityError()
                {
                    Code = "PasswordContains1234",
                    Description = "Sifre , ardisik sayi iceremez"
                });
            }

            if (errors.Count == 0) return Task.FromResult(IdentityResult.Success);
            else return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
        }
    }
}
