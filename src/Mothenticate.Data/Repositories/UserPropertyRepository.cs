using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public class UserPropertyRepository(MothenticateDbContext db) : IUserPropertyRepository
{
    public async Task<IReadOnlyList<UserProperty>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.UserProperties.OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ToListAsync(cancellationToken);

    public async Task<UserProperty?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.UserProperties.FindAsync([id], cancellationToken);

    public async Task<UserProperty> CreateAsync(UserProperty property, CancellationToken cancellationToken = default)
    {
        db.UserProperties.Add(property);
        await db.SaveChangesAsync(cancellationToken);
        return property;
    }

    public async Task UpdateAsync(UserProperty property, CancellationToken cancellationToken = default)
    {
        db.UserProperties.Update(property);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var property = await db.UserProperties.FindAsync([id], cancellationToken);
        if (property is not null)
        {
            db.UserProperties.Remove(property);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<UserPropertyValue>> GetValuesForUserAsync(string userId, CancellationToken cancellationToken = default)
        => await db.UserPropertyValues
            .Include(v => v.Property)
            .Where(v => v.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task SetValueAsync(string userId, int propertyId, string? value, CancellationToken cancellationToken = default)
    {
        var existing = await db.UserPropertyValues
            .FirstOrDefaultAsync(v => v.UserId == userId && v.PropertyId == propertyId, cancellationToken);

        if (existing is not null)
        {
            existing.Value = value;
            db.UserPropertyValues.Update(existing);
        }
        else
        {
            db.UserPropertyValues.Add(new UserPropertyValue
            {
                UserId = userId,
                PropertyId = propertyId,
                Value = value
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
