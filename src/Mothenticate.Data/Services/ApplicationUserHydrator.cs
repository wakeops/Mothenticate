using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Services;

/// <summary>
/// UserName and Email are not mapped columns on ApplicationUser — they live exclusively in
/// UserAttributeValue (attributes "username", "email"). Anything that loads an ApplicationUser
/// via a raw EF query (rather than through ApplicationUserStore) must hydrate these in-memory afterward.
/// </summary>
internal static class ApplicationUserHydrator
{
    public static Task HydrateAsync(MothenticateDbContext db, ApplicationUser? user, CancellationToken cancellationToken = default)
        => user is null ? Task.CompletedTask : HydrateManyAsync(db, [user], cancellationToken);

    public static async Task HydrateManyAsync(MothenticateDbContext db, IReadOnlyList<ApplicationUser> users, CancellationToken cancellationToken = default)
    {
        if (users.Count == 0)
        {
            return;
        }

        var ids = users.Select(u => u.Id).ToList();

        var values = await db.UserAttributeValues
            .Where(v => ids.Contains(v.UserId) &&
                (v.UserAttribute.Name == "username" || v.UserAttribute.Name == "email"))
            .Select(v => new { v.UserId, AttributeName = v.UserAttribute.Name, v.Value })
            .ToListAsync(cancellationToken);

        var byUser = values.ToLookup(v => v.UserId);

        foreach (var user in users)
        {
            foreach (var v in byUser[user.Id])
            {
                switch (v.AttributeName)
                {
                    case "username":
                        user.UserName = v.Value;
                        user.NormalizedUserName = v.Value?.ToUpperInvariant();
                        break;
                    case "email":
                        user.Email = v.Value;
                        user.NormalizedEmail = v.Value?.ToUpperInvariant();
                        break;
                }
            }
        }
    }
}
