using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;

namespace Mothenticate.UserManagement.Services;

public class UserPropertyService(IUserPropertyRepository userPropertyRepository) : IUserPropertyService
{
    public Task<IReadOnlyList<UserProperty>> GetAllDefinitionsAsync(CancellationToken cancellationToken = default)
        => userPropertyRepository.GetAllAsync(cancellationToken);

    public Task<UserProperty?> GetDefinitionByIdAsync(int id, CancellationToken cancellationToken = default)
        => userPropertyRepository.GetByIdAsync(id, cancellationToken);

    public async Task<UserProperty> CreateDefinitionAsync(string name, string displayName, PropertyType type,
        bool isRequired = false, bool isHidden = false, bool isReadOnly = false, CancellationToken cancellationToken = default)
    {
        var existing = (await userPropertyRepository.GetAllAsync(cancellationToken))
            .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            throw new InvalidOperationException($"A property with name '{name}' already exists.");
        }

        var property = new UserProperty
        {
            Name = name,
            DisplayName = displayName,
            Type = type,
            IsRequired = isRequired,
            IsHidden = isHidden,
            IsReadOnly = isReadOnly
        };

        return await userPropertyRepository.CreateAsync(property, cancellationToken);
    }

    public async Task UpdateDefinitionAsync(int id, string displayName, PropertyType type,
        bool isRequired, bool isHidden, bool isReadOnly, CancellationToken cancellationToken = default)
    {
        var property = await userPropertyRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Property '{id}' not found.");

        property.DisplayName = displayName;
        property.Type = type;
        property.IsRequired = isRequired;
        property.IsHidden = isHidden;
        property.IsReadOnly = isReadOnly;

        await userPropertyRepository.UpdateAsync(property, cancellationToken);
    }

    public Task DeleteDefinitionAsync(int id, CancellationToken cancellationToken = default)
        => userPropertyRepository.DeleteAsync(id, cancellationToken);

    public Task<IReadOnlyList<UserPropertyValue>> GetUserValuesAsync(string userId, CancellationToken cancellationToken = default)
        => userPropertyRepository.GetValuesForUserAsync(userId, cancellationToken);

    public Task SetUserValueAsync(string userId, int propertyId, string? value, CancellationToken cancellationToken = default)
        => userPropertyRepository.SetValueAsync(userId, propertyId, value, cancellationToken);
}
