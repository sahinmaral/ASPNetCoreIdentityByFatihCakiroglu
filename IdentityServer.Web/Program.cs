using IdentityServer.Web.ClaimProvider;
using IdentityServer.Web.Claims;
using IdentityServer.Web.Controllers;
using IdentityServer.Web.Extensions;
using IdentityServer.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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


builder.Services.Configure<EmailServiceConfigurationModel>(builder.Configuration.GetSection("Email"));

builder.Services.AddIdentityWithConfigurations();

builder.Services.ConfigureCookieOptions();

builder.Services.AddScoped<IClaimsTransformation, ClaimProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PremiumExchangeHandler>();

builder.Services.AddAuthorization(config =>
{
    config.AddPolicy("AnkaraPolicy", policy =>
    {
        policy.RequireClaim("city", "Ankara");
    });

    config.AddPolicy("IstanbulPolicy", policy =>
    {
        policy.RequireClaim("city", "Istanbul");
    });

    config.AddPolicy("ViolencePolicy", policy =>
    {
        policy.RequireClaim("violence", true.ToString());
    });
    config.AddPolicy("PremiumExchangePolicy", policy =>
    {
        policy.AddRequirements(new PremiumExchangeRequirement());
    });

});


builder.Services.AddAuthenticationFromThirdPartyApplications();

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

