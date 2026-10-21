using System.Globalization;
using AkarangaBank.Web.Data;
using AkarangaBank.Web.Models;
using AkarangaBank.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ---------- Base de données ----------
builder.Services.AddDbContext<AkarangaBankContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AkarangaBank")));

// ---------- Identity ----------
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AkarangaBankContext>()
.AddDefaultTokenProviders();

// ---------- Cookie d'authentification ----------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});

// ---------- MVC + Razor Pages ----------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// ---------- Localisation ----------
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[]
    {
        new CultureInfo("fr-BI"),
        new CultureInfo("en-US"),
        new CultureInfo("rn-BI")
    };

    options.DefaultRequestCulture = new RequestCulture("fr-BI");
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;

    // Providers dans l'ordre : Cookie > QueryString > Accept-Language
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
    options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
    options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
});

// ---------- Services métier ----------
builder.Services.AddScoped<AgenceService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<CompteService>();
builder.Services.AddScoped<ComptabiliteService>();
builder.Services.AddScoped<ConformiteService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ---------- Localisation (DOIT être entre UseRouting et UseAuthentication) ----------
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.MapRazorPages();

// ---------- Seed ----------
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var context = sp.GetRequiredService<AkarangaBankContext>();
    context.Database.EnsureCreated();
    await DataSeeder.SeedAsync(sp);
}

app.Run();

public partial class Program { }