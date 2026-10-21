using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace AkarangaBank.Web.Controllers;

public class CultureController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Set(string culture, string? returnUrl = null)
    {
        // Ligne de diagnostic : s'affiche dans le terminal quand le POST arrive
        Console.WriteLine($">>>>>> CULTURE SET REÇU : {culture}");

        if (string.IsNullOrWhiteSpace(culture))
            culture = "fr-BI";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                HttpOnly = false,
                Secure = Request.IsHttps
            }
        );

        // Ligne de diagnostic : confirme que le cookie a été posé
        Console.WriteLine($">>>>>> COOKIE POSÉ : {CookieRequestCultureProvider.DefaultCookieName}={CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture))}");

        return LocalRedirect(returnUrl ?? "/");
    }
}