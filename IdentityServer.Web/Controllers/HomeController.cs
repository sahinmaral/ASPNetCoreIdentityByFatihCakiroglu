using IdentityServer.Web.Helper;
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

        public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration config) : base(userManager, signInManager)
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

        public async Task<IActionResult> EmailConfirm(string userId, string token)
        {
            AppUser user = await _userManager.FindByIdAsync(userId);

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return CreateCustomAlert("Hesabiniz onaylanmistir", "Success", $"/{nameof(HomeController).Replace("Controller", "")}/{nameof(HomeController.Homepage)}");
            }
            else
            {
                return CreateCustomAlert("Bir hata meydana geldi. Lutfen daha sonra tekrar deneyiniz", "Danger", $"/{nameof(HomeController).Replace("Controller", "")}/{nameof(HomeController.Homepage)}");

            }

        }

        [HttpPost]

        public async Task<IActionResult> SignUp(UserSignUpViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser();
                user.Email = viewModel.Email;
                user.PhoneNumber = viewModel.PhoneNumber;
                user.UserName = viewModel.Username;
                user.City = viewModel.City;

                user.Picture = "";

                IdentityResult result = await _userManager.CreateAsync(user, viewModel.Password);

                if (result.Succeeded)
                {
                    string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    string link = Url.Action(nameof(EmailConfirm), nameof(HomeController).Replace("Controller", ""), new
                    {
                        userId = user.Id,
                        token = confirmationToken
                    }, protocol: HttpContext.Request.Scheme);


                    EmailConfirmationHelper.EmailConfirmationSendEmail(link, user.Email);

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
                        return View(viewModel);
                    }

                    if(!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("", $"Hesabiniz onaylanmamistir. Lutfen email adresinize gonderilen link uzerinden onaylayiniz");
                        return View(viewModel);
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

                        if (failedAttemps == Convert.ToInt32(_config["Constants:AccessFailedCount"]))
                        {
                            await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.Now.AddMinutes(20)));
                            ModelState.AddModelError("", $"Hesabiniz {failedAttemps} basarisiz giristen dolayi 20 dakika kadar kilitlenmistir. Lutfen daha sonra tekrar deneyiniz");
                            return View(viewModel);
                        }

                        else
                        {
                            ModelState.AddModelError("", "Gecersiz email veya sifre");
                            return View(viewModel);
                        }



                    }
                }
                else
                {
                    ModelState.AddModelError("", "Boyle bir kullanici bulunamadi");
                    return View(viewModel);
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
                if (user != null)
                {
                    string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                    string passwordResetLink = Url.Action("ResetPasswordConfirm", "Home", new
                    {
                        userId = user.Id,
                        token = passwordResetToken
                    }, HttpContext.Request.Scheme);

                    PasswordResetHelper.PasswordResetSendEmail(passwordResetLink, user.Email);

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
        public IActionResult ResetPasswordConfirm(string userId, string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordConfirm([Bind("PasswordNew")] UserResetPasswordViewModel viewModel)
        {
            ModelState.Remove(nameof(viewModel.Email));

            string token = TempData["token"].ToString();
            string userId = TempData["userId"].ToString();

            if (ModelState.IsValid)
            {

                AppUser user = await _userManager.FindByIdAsync(userId);

                if (user != null)
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

        public IActionResult AccessDenied(string returnUrl)
        {
            if (returnUrl.Contains(nameof(MemberController.PageWhereIncludesViolence)))
            {
                ViewBag.Message = "Bu sayfa siddet icerikli bir sayfa oldugundan dolayi 18 yasindan buyuk olmaniz gerekir";
            }
            else if (returnUrl.Contains(nameof(MemberController.PageWhereUserLivesAnkara)))
            {
                ViewBag.Message = "Bu sayfaya sadece sehir alani Ankara olanlar girebilir";
            }
            else if (returnUrl.Contains(nameof(MemberController.PageWhereUserLivesIstanbul)))
            {
                ViewBag.Message = "Bu sayfaya sadece sehir alani Istanbul olanlar girebilir";
            }
            else
            {
                ViewBag.Message = "Bu sayfaya erisim izniniz yoktur. Erisim izni almak icin site yoneticiyle gorusunuz";
            }

            return View();
        }
    }
}
