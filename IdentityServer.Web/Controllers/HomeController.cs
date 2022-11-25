using IdentityServer.Web.Helpers;
using IdentityServer.Web.Models;
using IdentityServer.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            if(_userManager.Users.Any(x => x.PhoneNumber == viewModel.PhoneNumber))
            {
                ModelState.AddModelError("", "Boyle bir telefon numarasi ile kayitli kullanici bulunmaktadir");
            }

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


                    await EmailConfirmationHelper.EmailConfirmationSendEmail(link, user.Email);

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
                        else
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
            return CreateCustomAlert("Yeni sifre belirlemek icin kayitli e-posta adresinizi yaziniz.Sifre degistirme linkini e-posta adresinize gonderecegiz", "Danger", $"/{nameof(HomeController).Replace("Controller", "")}/{nameof(HomeController.Login)}");
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

        public IActionResult LoginWithFacebook(string returnUrl)
        {
            string redirectUrl = Url.Action(nameof(ResponseFromThirdPartyIdentity), "Home", new
            {
                returnUrl = returnUrl
            });

            AuthenticationProperties property = _signInManager.ConfigureExternalAuthenticationProperties("Facebook", redirectUrl);
            return new ChallengeResult("Facebook", property);
        }

        public IActionResult LoginWithGoogle(string returnUrl)
        {
            string redirectUrl = Url.Action(nameof(ResponseFromThirdPartyIdentity), "Home", new
            {
                returnUrl = returnUrl
            });

            AuthenticationProperties property = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return new ChallengeResult("Google", property);
        }

        public IActionResult LoginWithMicrosoft(string returnUrl)
        {
            string redirectUrl = Url.Action(nameof(ResponseFromThirdPartyIdentity), "Home", new
            {
                returnUrl = returnUrl
            });

            AuthenticationProperties property = _signInManager.ConfigureExternalAuthenticationProperties("Microsoft", redirectUrl);
            return new ChallengeResult("Microsoft", property);
        }

        public async Task<IActionResult> ResponseFromThirdPartyIdentity(string? returnUrl="/Home/Homepage")
        {
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if(info == null)
            {
                return RedirectToAction(nameof(Login));
            }
            else
            {
                Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);
                if (signInResult.Succeeded)
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    AppUser user = new AppUser();
                    user.Email = info.Principal.FindFirst(ClaimTypes.Email).Value;

                    string externalUserId = info.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;

                    if (info.Principal.HasClaim(x=>x.Type == ClaimTypes.Name))
                    {
                        string username = info.Principal.FindFirst(ClaimTypes.Name).Value;
                        username = username.Replace(" ", "-").ToLower()+"-"+externalUserId;
                        user.UserName = username;
                    }
                    else
                    {
                        user.UserName = info.Principal.FindFirst(ClaimTypes.Email).Value;
                    }

                    // If this user is already signed in and logged with same email
                    // -> We have to allow to login this user 

                    AppUser loggedUser = await _userManager.FindByEmailAsync(user.Email);

                    if(loggedUser == null)
                    {
                        IdentityResult identityResult = await _userManager.CreateAsync(user);
                        if (identityResult.Succeeded)
                        {
                            // Login for third-party identities

                            IdentityResult loginIdentityResult = await _userManager.AddLoginAsync(user, info);

                            if (loginIdentityResult.Succeeded)
                            {
                                //await _signInManager.SignInAsync(user, true);

                                // To identify that user logged in from facebook

                                await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);

                                return Redirect(returnUrl);
                            }
                            else
                            {
                                return CreateCustomAlert("Bir hata meydana geldi. Lutfen tekrar deneyiniz", "Danger", $"/{nameof(HomeController).Replace("Controller", "")}/{nameof(HomeController.Login)}");
                            }
                        }
                        else
                        {
                            string errorContent = "";
                            List<string> errors = identityResult.Errors.Select(x => x.Description).ToList();
                            foreach (var error in errors)
                            {
                                errorContent += error;
                                errorContent += "\n";
                            }

                            return CreateCustomAlert(errorContent, "Danger", $"/{nameof(HomeController).Replace("Controller", "")}/{nameof(HomeController.Login)}");
                        }
                    }
                    else
                    {
                        IdentityResult loginIdentityResult = await _userManager.AddLoginAsync(loggedUser, info);
                        if (loginIdentityResult.Succeeded)
                        {

                            await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);

                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return CreateCustomAlert("Bir hata meydana geldi. Lutfen tekrar deneyiniz", "Danger", $"/{nameof(HomeController).Replace("Controller", "")}/{nameof(HomeController.Login)}");
                        }
                    }
                    
                }
            }
        }
    }
}
