using Microsoft.Extensions.Caching.Memory;
using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;

namespace Mothenticate.UserManagement.Services;

public class ClientScopeService(IClientScopeRepository clientScopeRepository, IMemoryCache cache) : IClientScopeService
{
    // Scope-mapper config is read on every authorize/token/userinfo/introspection request but changes
    // only when an admin edits it, so a short cache window trades a bounded propagation delay for
    // cutting a DB round trip out of every OIDC request.
    private static readonly TimeSpan MapperCacheDuration = TimeSpan.FromSeconds(30);

    public Task<IReadOnlyList<ClientScope>> GetAllAsync(CancellationToken cancellationToken = default)
        => clientScopeRepository.GetAllAsync(cancellationToken);

    public Task<ClientScope?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => clientScopeRepository.GetByIdAsync(id, cancellationToken);

    public async Task<ClientScope> CreateAsync(ClientScope scope, CancellationToken cancellationToken = default)
    {
        var existing = (await clientScopeRepository.GetAllAsync(cancellationToken))
            .FirstOrDefault(s => s.Name.Equals(scope.Name, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            throw new InvalidOperationException($"A client scope with name '{scope.Name}' already exists.");
        }

        return await clientScopeRepository.CreateAsync(scope, cancellationToken);
    }

    public Task UpdateAsync(ClientScope scope, CancellationToken cancellationToken = default)
        => clientScopeRepository.UpdateAsync(scope, cancellationToken);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => clientScopeRepository.DeleteAsync(id, cancellationToken);

    public Task<ClientScopeMapper> AddMapperAsync(ClientScopeMapper mapper, CancellationToken cancellationToken = default)
        => clientScopeRepository.AddMapperAsync(mapper, cancellationToken);

    public Task UpdateMapperAsync(ClientScopeMapper mapper, CancellationToken cancellationToken = default)
        => clientScopeRepository.UpdateMapperAsync(mapper, cancellationToken);

    public Task DeleteMapperAsync(int mapperId, CancellationToken cancellationToken = default)
        => clientScopeRepository.DeleteMapperAsync(mapperId, cancellationToken);

    public async Task<IReadOnlyList<ClientScopeMapper>> GetMappersByScopeNamesAsync(IReadOnlyList<string> scopeNames, CancellationToken cancellationToken = default)
    {
        if (scopeNames.Count == 0)
        {
            return [];
        }

        var cacheKey = $"{nameof(ClientScopeService)}:Mappers:{string.Join(',', scopeNames.OrderBy(s => s, StringComparer.Ordinal))}";
        if (cache.TryGetValue(cacheKey, out IReadOnlyList<ClientScopeMapper>? cached) && cached is not null)
        {
            return cached;
        }

        var mappers = await clientScopeRepository.GetMappersByScopeNamesAsync(scopeNames, cancellationToken);
        cache.Set(cacheKey, mappers, MapperCacheDuration);
        return mappers;
    }
}
