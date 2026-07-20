using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;

namespace Mothenticate.UserManagement.Services;

public class ClientScopeService(IClientScopeRepository clientScopeRepository) : IClientScopeService
{
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

    public Task<IReadOnlyList<ClientScopeMapper>> GetMappersByScopeNamesAsync(IReadOnlyList<string> scopeNames, CancellationToken cancellationToken = default)
        => clientScopeRepository.GetMappersByScopeNamesAsync(scopeNames, cancellationToken);
}
