using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Mothenticate.Controllers;

[Route("culture")]
public class CultureController : Controller
{
    [HttpPost("set")]
    [IgnoreAntiforgeryToken]
    public IActionResult Set([FromForm] string culture, [FromForm] string returnUrl = "/")
    {
        if (!string.IsNullOrEmpty(culture) &&
            LocalizationDefaults.SupportedLanguages.Any(l => l.Code == culture))
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
        }

        return LocalRedirect(returnUrl);
    }
}
