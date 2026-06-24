using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public interface IClientSecretRepository
{
    Task<IReadOnlyList<ClientSecret>> GetByApplicationIdAsync(string applicationId, CancellationToken cancellationToken = default);
    Task<ClientSecret?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ClientSecret> CreateAsync(ClientSecret secret, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
