using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public class UserAttributeRepository(MothenticateDbContext db) : IUserAttributeRepository
{
    public async Task<IReadOnlyList<UserAttribute>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.UserAttributes
            .Include(a => a.Scopes)
            .Include(a => a.Validators)
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Name)
            .ToListAsync(cancellationToken);

    public async Task<UserAttribute?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.UserAttributes
            .Include(a => a.Scopes)
            .Include(a => a.Validators)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<UserAttribute> CreateAsync(UserAttribute attribute, CancellationToken cancellationToken = default)
    {
        db.UserAttributes.Add(attribute);
        await db.SaveChangesAsync(cancellationToken);
        return attribute;
    }

    public async Task UpdateAsync(UserAttribute attribute, CancellationToken cancellationToken = default)
    {
        db.UserAttributes.Update(attribute);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var attribute = await db.UserAttributes.FindAsync([id], cancellationToken);
        if (attribute is not null)
        {
            db.UserAttributes.Remove(attribute);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateSortOrderAsync(IReadOnlyList<(int Id, int SortOrder)> order, CancellationToken cancellationToken = default)
    {
        var ids = order.Select(o => o.Id).ToList();
        var attributes = await db.UserAttributes.Where(a => ids.Contains(a.Id)).ToListAsync(cancellationToken);

        foreach (var (id, sortOrder) in order)
        {
            var attribute = attributes.FirstOrDefault(a => a.Id == id);
            if (attribute is not null)
            {
                attribute.SortOrder = sortOrder;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserAttributeValue>> GetValuesForUserAsync(string userId, CancellationToken cancellationToken = default)
        => await db.UserAttributeValues
            .Include(v => v.UserAttribute)
            .Where(v => v.UserId == userId)
            .OrderBy(v => v.UserAttributeId).ThenBy(v => v.Ordinal)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserAttribute>> GetAllWithUserValuesAsync(string userId, CancellationToken cancellationToken = default)
        => await db.UserAttributes
            .Include(a => a.Values.Where(v => v.UserId == userId))
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Name)
            .ToListAsync(cancellationToken);

    public async Task SetValuesAsync(string userId, int attributeId, IReadOnlyList<string?> values, CancellationToken cancellationToken = default)
    {
        var existing = await db.UserAttributeValues
            .Where(v => v.UserId == userId && v.UserAttributeId == attributeId)
            .ToListAsync(cancellationToken);

        db.UserAttributeValues.RemoveRange(existing);

        for (var i = 0; i < values.Count; i++)
        {
            db.UserAttributeValues.Add(new UserAttributeValue
            {
                UserId = userId,
                UserAttributeId = attributeId,
                Value = values[i],
                Ordinal = i
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
