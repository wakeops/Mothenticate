using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Services;

/// <summary>
/// UserName, Email, and DisplayName are not mapped columns on ApplicationUser (see ApplicationUserMapping) —
/// they live exclusively in UserAttributeValue rows for the "username"/"email"/"displayName" attributes.
/// This store persists those in-memory properties to UserAttributeValue after the base store writes the
/// core AspNetUsers row, and resolves/hydrates them back from UserAttributeValue on lookup.
/// </summary>
public class ApplicationUserStore(MothenticateDbContext context, IdentityErrorDescriber? describer = null)
    : UserStore<ApplicationUser, IdentityRole, MothenticateDbContext>(context, describer)
{
    public override async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var userName = user.UserName;
        var email = user.Email;

        var result = await base.CreateAsync(user, cancellationToken);
        if (!result.Succeeded)
        {
            return result;
        }

        await SetAttributeValueAsync(user.Id, "username", userName, cancellationToken);
        await SetAttributeValueAsync(user.Id, "email", email, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        return result;
    }

    public override async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var result = await base.UpdateAsync(user, cancellationToken);
        if (!result.Succeeded)
        {
            return result;
        }

        await SetAttributeValueAsync(user.Id, "username", user.UserName, cancellationToken);
        await SetAttributeValueAsync(user.Id, "email", user.Email, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        return result;
    }

    public override async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await base.FindByIdAsync(userId, cancellationToken);
        await ApplicationUserHydrator.HydrateAsync(Context, user, cancellationToken);
        return user;
    }

    public override async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        var userId = await Context.UserAttributeValues
            .Where(v => v.UserAttribute.Name == "username" && v.Value != null && v.Value.ToUpper() == normalizedUserName)
            .Select(v => v.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (userId is null)
        {
            return null;
        }

        return await FindByIdAsync(userId, cancellationToken);
    }

    public override async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        var userId = await Context.UserAttributeValues
            .Where(v => v.UserAttribute.Name == "email" && v.Value != null && v.Value.ToUpper() == normalizedEmail)
            .Select(v => v.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (userId is null)
        {
            return null;
        }

        return await FindByIdAsync(userId, cancellationToken);
    }

    private async Task SetAttributeValueAsync(string userId, string attributeName, string? value, CancellationToken cancellationToken)
    {
        var attribute = await Context.UserAttributes.FirstOrDefaultAsync(a => a.Name == attributeName, cancellationToken);
        if (attribute is null)
        {
            return;
        }

        var existing = await Context.UserAttributeValues
            .FirstOrDefaultAsync(v => v.UserId == userId && v.UserAttributeId == attribute.Id && v.Ordinal == 0, cancellationToken);

        if (existing is not null)
        {
            existing.Value = value;
        }
        else
        {
            Context.UserAttributeValues.Add(new UserAttributeValue
            {
                UserId = userId,
                UserAttributeId = attribute.Id,
                Value = value,
                Ordinal = 0
            });
        }
    }
}
