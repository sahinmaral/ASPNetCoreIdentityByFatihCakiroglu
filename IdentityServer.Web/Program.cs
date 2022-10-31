using IdentityServer.Web.Controllers;
using IdentityServer.Web.CustomValidation.MicrosoftIdentity;
using IdentityServer.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();
;

/* Start of localizer implementation */

builder.Services.AddLocalization(opt => opt.ResourcesPath = "Resources");

const string trTRCulture = "tr-TR";

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var culture = new List<CultureInfo> {
                new CultureInfo(trTRCulture),
                new CultureInfo("en-US"),        
    };
    options.DefaultRequestCulture = new RequestCulture(culture: trTRCulture, uiCulture: trTRCulture);
    options.SupportedCultures = culture;
    options.SupportedUICultures = culture;
});

/* End of localizer implementation */


builder.Services.AddDbContext<AppIdentityDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString"));
});

builder.Services.AddIdentity<AppUser, AppRole>(opt =>
{
    opt.Password.RequiredLength = 4;
    opt.User.RequireUniqueEmail = true;
})
    .AddPasswordValidator<CustomPasswordValidator>()
    .AddUserValidator<CustomUserValidator>()
    .AddErrorDescriber<CustomIdentityErrorDescriber>()
    .AddEntityFrameworkStores<AppIdentityDbContext>()
    .AddDefaultTokenProviders();

CookieBuilder cookieBuilder = new CookieBuilder();
cookieBuilder.Name = "MyBlog";
cookieBuilder.HttpOnly = false; 


// CSRF 
cookieBuilder.SameSite = SameSiteMode.Lax;

// HTTPS -> Always
// HTTP -> SameAsRequest
// No Configuration -> None
cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = $"/{nameof(HomeController).Replace("Controller", "")}/{nameof(HomeController.Login)}";
    opt.LogoutPath = $"/{nameof(HomeController).Replace("Controller", "")}/{nameof(HomeController.Logout)}";
    opt.Cookie = cookieBuilder;
    opt.SlidingExpiration = true;
    opt.ExpireTimeSpan = TimeSpan.FromDays(1);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

else
{
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePages();
}

app.UseAuthentication();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Homepage}/{id?}");

app.Run();
