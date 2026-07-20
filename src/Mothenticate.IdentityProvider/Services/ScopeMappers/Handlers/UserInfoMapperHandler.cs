using System.Security.Claims;
using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers.Handlers;

public static class UserInfoMapperHandler
{
    public static async Task HandleAsync(IUserInfoMapper mapper, Dictionary<string, string> config, ClaimsIdentity identity, ApplicationUser user, IReadOnlyCollection<UserAttribute> userAttributes, Dictionary<string, string> userInfo, CancellationToken cancellationToken)
    {
        if (GetConfigFlag(config, "IncludeUserInfo"))
        {
            var mapUserInfo = await mapper.MapUserInfoAsync(identity, user, userAttributes, config, cancellationToken);
            foreach (var item in mapUserInfo)
            {
                userInfo[item.Key] = item.Value;
            }
        }
    }

    private static bool GetConfigFlag(Dictionary<string, string> config, string key)
        => config.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) && parsed;
}