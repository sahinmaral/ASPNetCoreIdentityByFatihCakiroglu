using IdentityServer.Web.Controllers;
using IdentityServer.Web.CustomValidation.MicrosoftIdentity;
using IdentityServer.Web.Helpers;
using IdentityServer.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Web.Extensions
{

    public static class IdentityConfigurationExtension
    {

        /// <summary>
        /// Adds custom password and user validator , error describer and default configurations
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        public static void AddIdentityWithConfigurations(this IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.AddIdentity<AppUser, AppRole>(opt =>
            {
                opt.Password.RequiredLength = 4;
                opt.User.RequireUniqueEmail = true;
                opt.User.AllowedUserNameCharacters =
                        "abcçdefgğhijklmnoöpqrsştuüvwxyzABCÇDEFGĞHIİJKLMNOÖPQRSŞTUÜVWXYZ0123456789-._";
            })
                .AddPasswordValidator<CustomPasswordValidator>()
                .AddUserValidator<CustomUserValidator>()
                .AddErrorDescriber<CustomIdentityErrorDescriber>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

            serviceDescriptors.Configure<DataProtectionTokenProviderOptions>(config =>
            {
                config.TokenLifespan = TimeSpan.FromHours(1);
            });
        }

        /// <summary>
        /// Adds authentication configurations to allows to login facebook , google and microsoft accounts
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        public static void AddAuthenticationFromThirdPartyApplications(this IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.AddAuthentication()
                .AddFacebook(config =>
                {
                    var facebookConfigModel = ThirdPartyIdentityHelper.GetConfigurationModel("Facebook");

                    config.AppId = facebookConfigModel.Id;
                    config.AppSecret = facebookConfigModel.Secret;

                })
                .AddGoogle(config =>
                {
                    var googleConfigurationModel = ThirdPartyIdentityHelper.GetConfigurationModel("Google");
                    config.ClientId = googleConfigurationModel.Id;
                    config.ClientSecret = googleConfigurationModel.Secret;
                })
                .AddMicrosoftAccount(config =>
                {
                    var microsoftConfigurationModel = ThirdPartyIdentityHelper.GetConfigurationModel("Microsoft");
                    config.ClientId = microsoftConfigurationModel.Id;
                    config.ClientSecret = microsoftConfigurationModel.Secret;
                });
        }

        /// <summary>
        /// Creates CookieBuilder , adds to configureApplicationCookie , configures options of cookie
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        public static void ConfigureCookieOptions(this IServiceCollection serviceDescriptors)
        {
            CookieBuilder cookieBuilder = new CookieBuilder();
            cookieBuilder.Name = "MyBlog";
            cookieBuilder.HttpOnly = false;


            // CSRF 
            cookieBuilder.SameSite = SameSiteMode.Lax;

            // HTTPS -> Always
            // HTTP -> SameAsRequest
            // No Configuration -> None
            cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;

            serviceDescriptors.ConfigureApplicationCookie(opt =>
            {
                opt.LoginPath = $"/{nameof(HomeController).Replace("Controller", "")}/{nameof(HomeController.Login)}";
                opt.LogoutPath = $"/{nameof(HomeController).Replace("Controller", "")}/{nameof(HomeController.Logout)}";
                opt.Cookie = cookieBuilder;
                opt.SlidingExpiration = true;
                opt.ExpireTimeSpan = TimeSpan.FromDays(1);
                opt.AccessDeniedPath = $"/{nameof(HomeController).Replace("Controller", "")}/{nameof(HomeController.AccessDenied)}";
            });
        }
    }
}
