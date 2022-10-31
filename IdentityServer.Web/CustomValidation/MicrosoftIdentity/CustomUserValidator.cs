﻿using IdentityServer.Web.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace IdentityServer.Web.CustomValidation.MicrosoftIdentity
{
    public class CustomUserValidator : IUserValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            List<IdentityError> errors = new List<IdentityError>();

            string[] digits = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            if (digits.Contains(user.UserName[0].ToString()))
            {
                errors.Add(new IdentityError()
                {
                    Code = "FirstLetterOfUsernameContainsDigit",
                    Description = "Kullanici adinin ilk karakteri sayisal karakter iceremez"
                });
            }

            if (errors.Count == 0) return Task.FromResult(IdentityResult.Success);
            else return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
        }
    }
}
