using IdentityServer.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mapster;
using IdentityServer.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using IdentityServer.Web.Enums;

namespace IdentityServer.Web.Controllers
{
    [Authorize]
    public class MemberController : BaseController
    {
        public MemberController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : base(userManager, signInManager)
        {
        }

        public async Task<IActionResult> Homepage()
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
        public async Task<IActionResult> UpdateInformations()
        {
            AppUser user = CurrentUser;

            UpdateUserInformationsViewModel viewModel = user.Adapt<UpdateUserInformationsViewModel>();

            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInformations(UpdateUserInformationsViewModel viewModel,IFormFile userPicture)
        {
            ModelState.Remove(nameof(userPicture));
            ModelState.Remove(nameof(viewModel.Picture));

            if (ModelState.IsValid)
            {
                AppUser user = CurrentUser;

                if(userPicture != null && userPicture.Length > 0)
                {
                    string oldUserImageFileName = user.Picture;

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName);


                    string path = Path.Combine(Directory.GetCurrentDirectory()+ @"\wwwroot\images\userPictures\"+ fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await userPicture.CopyToAsync(stream);
                        user.Picture = fileName;
                    }

                    System.IO.File.Delete(Directory.GetCurrentDirectory() + @"\wwwroot\images\userPictures\" + oldUserImageFileName);
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
