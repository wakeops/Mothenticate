using Mothenticate.Data.Entities;

namespace Mothenticate.UserManagement.Services;

public interface IClientScopeService
{
    Task<IReadOnlyList<ClientScope>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ClientScope?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ClientScope> CreateAsync(ClientScope scope, CancellationToken cancellationToken = default);
    Task UpdateAsync(ClientScope scope, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<ClientScopeMapper> AddMapperAsync(ClientScopeMapper mapper, CancellationToken cancellationToken = default);
    Task UpdateMapperAsync(ClientScopeMapper mapper, CancellationToken cancellationToken = default);
    Task DeleteMapperAsync(int mapperId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientScopeMapper>> GetMappersByScopeNamesAsync(IReadOnlyList<string> scopeNames, CancellationToken cancellationToken = default);
}
