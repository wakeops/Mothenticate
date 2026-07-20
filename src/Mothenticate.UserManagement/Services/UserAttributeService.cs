using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;

namespace Mothenticate.UserManagement.Services;

public class UserAttributeService(IUserAttributeRepository userAttributeRepository) : IUserAttributeService
{
    public Task<IReadOnlyList<UserAttribute>> GetAllDefinitionsAsync(CancellationToken cancellationToken = default)
        => userAttributeRepository.GetAllAsync(cancellationToken);

    public Task<UserAttribute?> GetDefinitionByIdAsync(int id, CancellationToken cancellationToken = default)
        => userAttributeRepository.GetByIdAsync(id, cancellationToken);

    public async Task<UserAttribute> CreateDefinitionAsync(UserAttribute attribute, CancellationToken cancellationToken = default)
    {
        var existing = (await userAttributeRepository.GetAllAsync(cancellationToken))
            .FirstOrDefault(a => a.Name.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            throw new InvalidOperationException($"An attribute with name '{attribute.Name}' already exists.");
        }

        return await userAttributeRepository.CreateAsync(attribute, cancellationToken);
    }

    public async Task UpdateDefinitionAsync(UserAttribute attribute, CancellationToken cancellationToken = default)
    {
        var existing = await userAttributeRepository.GetByIdAsync(attribute.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Attribute '{attribute.Id}' not found.");

        existing.DisplayName = attribute.DisplayName;
        existing.InputType = attribute.InputType;
        existing.IsMultivalued = attribute.IsMultivalued;
        existing.DefaultValue = attribute.DefaultValue;
        existing.EnabledWhen = attribute.EnabledWhen;
        existing.IsRequired = attribute.IsRequired;
        existing.RequiredFor = attribute.RequiredFor;
        existing.RequiredWhen = attribute.RequiredWhen;
        existing.CanEditUser = attribute.CanEditUser;
        existing.CanEditAdmin = attribute.CanEditAdmin;
        existing.CanViewUser = attribute.CanViewUser;
        existing.CanViewAdmin = attribute.CanViewAdmin;

        existing.Scopes.Clear();
        foreach (var scope in attribute.Scopes)
        {
            existing.Scopes.Add(scope);
        }

        existing.Validators.Clear();
        foreach (var validator in attribute.Validators)
        {
            existing.Validators.Add(validator);
        }

        await userAttributeRepository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteDefinitionAsync(int id, CancellationToken cancellationToken = default)
    {
        var attribute = await userAttributeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"Attribute '{id}' not found.");

        if (attribute.IsBuiltIn)
        {
            throw new InvalidOperationException($"Attribute '{attribute.Name}' cannot be deleted.");
        }

        await userAttributeRepository.DeleteAsync(id, cancellationToken);
    }

    public Task ReorderAsync(IReadOnlyList<(int Id, int SortOrder)> order, CancellationToken cancellationToken = default)
        => userAttributeRepository.UpdateSortOrderAsync(order, cancellationToken);

    public Task<IReadOnlyList<UserAttributeValue>> GetUserValuesAsync(string userId, CancellationToken cancellationToken = default)
        => userAttributeRepository.GetValuesForUserAsync(userId, cancellationToken);

    public Task<IReadOnlyList<UserAttribute>> GetAllWithUserValuesAsync(string userId, CancellationToken cancellationToken = default)
        => userAttributeRepository.GetAllWithUserValuesAsync(userId, cancellationToken);

    public Task SetUserValuesAsync(string userId, int attributeId, IReadOnlyList<string?> values, CancellationToken cancellationToken = default)
        => userAttributeRepository.SetValuesAsync(userId, attributeId, values, cancellationToken);

    public async Task SetUserValueByNameAsync(string userId, string attributeName, string? value, CancellationToken cancellationToken = default)
    {
        var attribute = (await userAttributeRepository.GetAllAsync(cancellationToken))
            .FirstOrDefault(a => a.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase));

        if (attribute is not null)
        {
            await userAttributeRepository.SetValuesAsync(userId, attribute.Id, [value], cancellationToken);
        }
    }
}
