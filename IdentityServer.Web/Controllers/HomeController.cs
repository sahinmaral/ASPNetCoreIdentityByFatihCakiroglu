using IdentityServer.Web.Helpers;
using IdentityServer.Web.Models;
using IdentityServer.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace IdentityServer.Web.Controllers
{
    public class HomeController : BaseController
    {

        private readonly IConfiguration _config;    
        public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration config):base(userManager,signInManager)
        {
            _config = config;
        }

        public IActionResult Homepage()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> SignUpAsync(UserSignUpViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser();
                user.Email = viewModel.Email;
                user.PhoneNumber = viewModel.PhoneNumber;
                user.UserName = viewModel.Username;

                IdentityResult result = await _userManager.CreateAsync(user, viewModel.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View();
                }
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            TempData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByEmailAsync(viewModel.Email);
                if (user != null)
                {
                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        ModelState.AddModelError("", $"Hesabiniz {user.LockoutEnd} kadar kilitlenmistir. Lutfen daha sonra tekrar deneyiniz");
                    }

                    await _signInManager.SignOutAsync();

                    Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user, viewModel.Password, viewModel.RememberMe, false);

                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);

                        if (TempData["ReturnUrl"] != null)
                            return Redirect(TempData["ReturnUrl"].ToString());

                        return RedirectToAction(nameof(MemberController.Homepage), nameof(MemberController).Replace("Controller", ""));
                    }

                    else
                    {
                        await _userManager.AccessFailedAsync(user);

                        int failedAttemps = await _userManager.GetAccessFailedCountAsync(user);

                        if(failedAttemps == Convert.ToInt32(_config["Constants:AccessFailedCount"]))
                        {
                            await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.Now.AddMinutes(20)));
                            ModelState.AddModelError("", $"Hesabiniz {failedAttemps} basarisiz giristen dolayi 20 dakika kadar kilitlenmistir. Lutfen daha sonra tekrar deneyiniz");
                        }

                        else
                        {
                            ModelState.AddModelError("", "Gecersiz email veya sifre");
                        }


                        
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Boyle bir kullanici bulunamadi");
                }



            }
            return View(viewModel);

        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            string passwordResetInfo = "Yeni sifre belirlemek icin kayitli e-posta adresinizi yaziniz.Sifre degistirme linkini e-posta adresinize gonderecegiz.";

            if (TempData["PasswordResetInfo"] == null)
            {
                TempData["PasswordResetInfo"] = passwordResetInfo;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(UserResetPasswordViewModel viewModel)
        {
            ModelState.Remove(nameof(viewModel.PasswordNew));

            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByEmailAsync(viewModel.Email);
                if(user != null)
                {
                    string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                    string passwordResetLink = Url.Action("ResetPasswordConfirm","Home",new
                    {
                        userId = user.Id,
                        token = passwordResetToken
                    },HttpContext.Request.Scheme);

                    PasswordResetHelper.PasswordResetSendEmail(passwordResetLink,user.Email);

                    TempData["PasswordResetInfo"] = "Sifre degistirme linkiniz e-posta adresinize gonderilmistir. Eger gelen kutusunda yoksa spam/gereksiz kismini kontrol ediniz.";

                    return RedirectToAction(nameof(ResetPassword));

                }
                else
                {
                    ModelState.AddModelError("", "Boyle bir kullanici bulunamadi");
                }

            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirm(string userId,string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordConfirm([Bind("PasswordNew")]UserResetPasswordViewModel viewModel)
        {
            ModelState.Remove(nameof(viewModel.Email));

            string token = TempData["token"].ToString();
            string userId = TempData["userId"].ToString();

            if (ModelState.IsValid)
            {

                AppUser user = await _userManager.FindByIdAsync(userId);

                if(user != null)
                {
                    IdentityResult result = await _userManager.ResetPasswordAsync(user, token, viewModel.PasswordNew);

                    if (result.Succeeded)
                    {
                        // Kullanicinin bilgilerini (onemli olanlar) degistirdigimiz zaman
                        // security stamp kisminin guncellenmesi gerekir.

                        await _userManager.UpdateSecurityStampAsync(user);

                        TempData["PasswordResetInfo"] = "Sifreniz basariyla yenilenmistir. Yeni sifreniz ile giris yapabilirsiniz";

                        return RedirectToAction(nameof(Login));
                    }
                    else
                    {
                        AddModelError(result);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Boyle bir kullanici bulunamadi");
                }
            }

            TempData["userId"] = userId;
            TempData["token"] = token;
            return View();
        }

        public void Logout()
        {
            _signInManager.SignOutAsync();
        }
    }
}
