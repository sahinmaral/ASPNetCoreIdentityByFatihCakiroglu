using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace IdentityServer.Web.CustomValidation.MicrosoftIdentity
{
    public class CustomIdentityErrorDescriber:IdentityErrorDescriber
    {
        private readonly IStringLocalizer<CustomIdentityErrorDescriber> _stringLocalizer;
        public CustomIdentityErrorDescriber(IStringLocalizer<CustomIdentityErrorDescriber> stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }
        public override IdentityError InvalidUserName(string userName)
        {
            return new IdentityError()
            {
                Code = "InvalidUserName",
                Description = _stringLocalizer["InvalidUserName",userName]
            };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError()
            {
                Code = "DuplicateEmail",
                Description = _stringLocalizer["DuplicateEmail", email]
            };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError()
            {
                Code = "PasswordTooShort",
                Description = _stringLocalizer["PasswordTooShort", length]
            };
        }
    }
}
