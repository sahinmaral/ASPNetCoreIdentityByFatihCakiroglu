using IdentityServer.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mapster;
using IdentityServer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using IdentityServer.Web.Enums;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Web.Controllers
{
    [Authorize]
    public class MemberController : BaseController
    {
        public MemberController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : base(userManager, signInManager)
        {
        }

        [Authorize(Roles = "Editor")]
        public IActionResult EditorHomepage()
        {
            return View();
        }

        [Authorize(Roles = "Manager")]
        public IActionResult ManagerHomepage()
        {
            return View();
        }


        [Authorize(Policy = "IstanbulPolicy")]
        public IActionResult PageWhereUserLivesIstanbul()
        {
            return View();
        }

        [Authorize(Policy = "AnkaraPolicy")]
        public IActionResult PageWhereUserLivesAnkara()
        {
            return View();
        }

        [Authorize(Policy = "ViolencePolicy")]
        public IActionResult PageWhereIncludesViolence()
        {
            return View();
        }

        public async Task<IActionResult> PremiumExchangePageRedirect()
        {
            bool result = User.HasClaim(x => x.Type == "PremiumExchangeExpireDate");

            if (!result)
            {
                Claim exchangeClaim = new Claim("PremiumExchangeExpireDate", DateTime.Now.AddDays(30).ToString("MM.dd.yyyy"), ClaimValueTypes.String, "Internal");
                await _userManager.AddClaimAsync(CurrentUser, exchangeClaim);

                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(CurrentUser, true);
            }


            return RedirectToAction(nameof(PremiumExchangePage));
        }

        [Authorize(Policy = "PremiumExchangePolicy")]
        public IActionResult PremiumExchangePage()
        {
            return View();
        }

        public IActionResult Homepage()
        {
            AppUser user = CurrentUser;

            UserProfileViewModel viewModel = user.Adapt<UserProfileViewModel>();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(UpdatePasswordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = CurrentUser;
                bool confirmPasswordResult = await _userManager.CheckPasswordAsync(user, viewModel.PasswordOld);
                if (confirmPasswordResult)
                {
                    IdentityResult result = await _userManager.ChangePasswordAsync(user, viewModel.PasswordOld, viewModel.PasswordNew);
                    if (result.Succeeded)
                    {

                        await _userManager.UpdateSecurityStampAsync(user);

                        await _signInManager.SignOutAsync();
                        await _signInManager.PasswordSignInAsync(user, viewModel.PasswordNew, true, false);

                        ViewBag.Status = "Sifreniz guncellenmistir.";
                    }
                    else
                    {
                        AddModelError(result);
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(viewModel.PasswordOld), "Eski sifreniz yanlis");
                }

            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult UpdateInformations()
        {
            AppUser user = CurrentUser;

            UpdateUserInformationsViewModel viewModel = user.Adapt<UpdateUserInformationsViewModel>();

            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInformations(UpdateUserInformationsViewModel viewModel, IFormFile userPicture)
        {
            AppUser user = CurrentUser;

            ModelState.Remove(nameof(userPicture));
            ModelState.Remove(nameof(viewModel.Picture));

            if (ModelState.IsValid)
            {  
                if(viewModel.PhoneNumber != await _userManager.GetPhoneNumberAsync(user))
                {
                    if (_userManager.Users.Any(x => x.PhoneNumber == viewModel.PhoneNumber))
                    {
                        ModelState.AddModelError("", "Boyle bir telefon numarasi ile kayitli kullanici bulunmaktadir");
                        ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));
                        return View(viewModel);
                    }

                }

                if(user.Picture != string.Empty)
                {
                    System.IO.File.Delete(Directory.GetCurrentDirectory() + @"\wwwroot\images\userPictures\" + user.Picture);
                }

                if (userPicture != null && userPicture.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName);


                    string path = Path.Combine(Directory.GetCurrentDirectory() + @"\wwwroot\images\userPictures\" + fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await userPicture.CopyToAsync(stream);
                        user.Picture = fileName;
                    }                 
                }

                user.UserName = viewModel.UserName;

                user.PhoneNumber = viewModel.PhoneNumber;

                user.Email = viewModel.Email;

                user.City = viewModel.City;
                user.BirthDate = viewModel.BirthDate;
                user.Gender = (int)viewModel.Gender;

                IdentityResult result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(user, true);

                    ViewBag.Status = "Kullanici bilgileriniz guncellenmistir.";
                }
                else
                {
                    AddModelError(result);
                }
            }

            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));
            return View(viewModel);
        }



    }
}
