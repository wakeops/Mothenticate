using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Mothenticate.UserManagement.Services;

namespace Mothenticate;

public class DbDefaultCultureProvider : IRequestCultureProvider
{
    public async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var settingsService = httpContext.RequestServices.GetService<IAppSettingsService>();
        if (settingsService is null) return null;

        var settings = await settingsService.GetAsync();
        var lang = settings.DefaultLanguage;

        return string.IsNullOrEmpty(lang) ? null : new ProviderCultureResult(lang);
    }
}
