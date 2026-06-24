using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public class ClientSecretRepository(MothenticateDbContext db) : IClientSecretRepository
{
    public async Task<IReadOnlyList<ClientSecret>> GetByApplicationIdAsync(string applicationId, CancellationToken cancellationToken = default)
        => await db.ClientSecrets
            .Where(s => s.ApplicationId == applicationId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<ClientSecret?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await db.ClientSecrets.FindAsync([id], cancellationToken);

    public async Task<ClientSecret> CreateAsync(ClientSecret secret, CancellationToken cancellationToken = default)
    {
        db.ClientSecrets.Add(secret);
        await db.SaveChangesAsync(cancellationToken);
        return secret;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var secret = await db.ClientSecrets.FindAsync([id], cancellationToken);
        if (secret is not null)
        {
            db.ClientSecrets.Remove(secret);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
