using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public class ClientScopeRepository(MothenticateDbContext db) : IClientScopeRepository
{
    public async Task<IReadOnlyList<ClientScope>> GetAllAsync(CancellationToken cancellationToken = default)
        => await db.ClientScopes.OrderBy(s => s.Name).ToListAsync(cancellationToken);

    public async Task<ClientScope?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.ClientScopes
            .Include(s => s.Mappers)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<ClientScope> CreateAsync(ClientScope scope, CancellationToken cancellationToken = default)
    {
        db.ClientScopes.Add(scope);
        await db.SaveChangesAsync(cancellationToken);
        return scope;
    }

    public async Task UpdateAsync(ClientScope scope, CancellationToken cancellationToken = default)
    {
        db.ClientScopes.Update(scope);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var scope = await db.ClientScopes.FindAsync([id], cancellationToken);
        if (scope is not null)
        {
            db.ClientScopes.Remove(scope);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<ClientScopeMapper> AddMapperAsync(ClientScopeMapper mapper, CancellationToken cancellationToken = default)
    {
        db.ClientScopeMappers.Add(mapper);
        await db.SaveChangesAsync(cancellationToken);
        return mapper;
    }

    public async Task UpdateMapperAsync(ClientScopeMapper mapper, CancellationToken cancellationToken = default)
    {
        db.ClientScopeMappers.Update(mapper);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteMapperAsync(int mapperId, CancellationToken cancellationToken = default)
    {
        var mapper = await db.ClientScopeMappers.FindAsync([mapperId], cancellationToken);
        if (mapper is not null)
        {
            db.ClientScopeMappers.Remove(mapper);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<ClientScopeMapper>> GetMappersByScopeNamesAsync(IReadOnlyList<string> scopeNames, CancellationToken cancellationToken = default)
        => await db.ClientScopeMappers
            .Include(m => m.ClientScope)
            .Where(m => scopeNames.Contains(m.ClientScope.Name))
            .ToListAsync(cancellationToken);
}
