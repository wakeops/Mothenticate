using Microsoft.EntityFrameworkCore;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public class IdentityProviderRepository(MothenticateDbContext db) : IIdentityProviderRepository
{
    public async Task<IReadOnlyList<IdentityProviderType>> GetProviderTypesAsync(CancellationToken cancellationToken = default)
        => await db.IdentityProviderTypes
            .Include(t => t.DefaultConfiguration)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

    public async Task UpsertProviderTypeAsync(string name, ProtocolType protocolType, string? defaultProperties, CancellationToken cancellationToken = default)
    {
        var existing = await db.IdentityProviderTypes
            .Include(t => t.DefaultConfiguration)
            .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);

        if (existing is null)
        {
            IdentityProviderConfiguration? config = null;
            if (defaultProperties is not null)
            {
                config = new IdentityProviderConfiguration { Properties = defaultProperties };
                db.IdentityProviderConfigurations.Add(config);
                await db.SaveChangesAsync(cancellationToken);
            }

            db.IdentityProviderTypes.Add(new IdentityProviderType
            {
                Name = name,
                ProtocolType = protocolType,
                DefaultConfigurationId = config?.Id,
            });
        }
        else
        {
            existing.Name = name;
            existing.ProtocolType = protocolType;

            if (defaultProperties is not null && existing.DefaultConfiguration is not null)
            {
                existing.DefaultConfiguration.Properties = defaultProperties;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<IdentityProvider>> GetProvidersAsync(CancellationToken cancellationToken = default)
        => await db.IdentityProviders
            .Include(p => p.ProviderType).ThenInclude(t => t.DefaultConfiguration)
            .Include(p => p.Configuration)
            .OrderBy(p => p.DisplayOrder.HasValue ? 0 : 1)
            .ThenBy(p => p.DisplayOrder)
            .ThenBy(p => p.DisplayName)
            .ToListAsync(cancellationToken);

    public async Task<IdentityProvider?> GetProviderAsync(int id, CancellationToken cancellationToken = default)
        => await db.IdentityProviders
            .Include(p => p.ProviderType).ThenInclude(t => t.DefaultConfiguration)
            .Include(p => p.Configuration)
            .Include(p => p.Mappers)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IdentityProvider?> GetProviderByAliasAsync(string alias, CancellationToken cancellationToken = default)
        => await db.IdentityProviders
            .Include(p => p.ProviderType).ThenInclude(t => t.DefaultConfiguration)
            .Include(p => p.Configuration)
            .Include(p => p.Mappers)
            .FirstOrDefaultAsync(p => p.Alias == alias, cancellationToken);

    public async Task<IdentityProvider> CreateProviderAsync(IdentityProvider provider, CancellationToken cancellationToken = default)
    {
        if (provider.Configuration is not null)
        {
            db.IdentityProviderConfigurations.Add(provider.Configuration);
            await db.SaveChangesAsync(cancellationToken);
            provider.ConfigurationId = provider.Configuration.Id;
            provider.Configuration = null;
        }

        db.IdentityProviders.Add(provider);
        await db.SaveChangesAsync(cancellationToken);
        return provider;
    }

    public async Task UpdateProviderAsync(IdentityProvider provider, CancellationToken cancellationToken = default)
    {
        if (provider.Configuration is not null)
        {
            db.IdentityProviderConfigurations.Update(provider.Configuration);
        }
        db.IdentityProviders.Update(provider);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteProviderAsync(int id, CancellationToken cancellationToken = default)
    {
        var provider = await db.IdentityProviders
            .Include(p => p.Configuration)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (provider is not null)
        {
            var instanceConfig = provider.Configuration;
            db.IdentityProviders.Remove(provider);
            await db.SaveChangesAsync(cancellationToken);

            if (instanceConfig is not null)
            {
                db.IdentityProviderConfigurations.Remove(instanceConfig);
                await db.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public async Task SetProviderEnabledAsync(int id, bool enabled, CancellationToken cancellationToken = default)
    {
        var provider = await db.IdentityProviders.FindAsync([id], cancellationToken);
        if (provider is not null)
        {
            provider.IsEnabled = enabled;
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IdentityProviderMapper> AddMapperAsync(IdentityProviderMapper mapper, CancellationToken cancellationToken = default)
    {
        db.IdentityProviderMappers.Add(mapper);
        await db.SaveChangesAsync(cancellationToken);
        return mapper;
    }

    public async Task UpdateMapperAsync(IdentityProviderMapper mapper, CancellationToken cancellationToken = default)
    {
        db.IdentityProviderMappers.Update(mapper);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteMapperAsync(int mapperId, CancellationToken cancellationToken = default)
    {
        var mapper = await db.IdentityProviderMappers.FindAsync([mapperId], cancellationToken);
        if (mapper is not null)
        {
            db.IdentityProviderMappers.Remove(mapper);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
