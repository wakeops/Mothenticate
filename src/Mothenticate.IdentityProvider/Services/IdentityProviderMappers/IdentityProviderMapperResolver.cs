using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.IdentityProviderMappers.Abstract;

namespace Mothenticate.IdentityProvider.Services.IdentityProviderMappers;

public class IdentityProviderMapperResolver(IEnumerable<IIdentityProviderMapper> mappers) : IIdentityProviderMapperResolver
{
    private readonly Dictionary<IdentityProviderMapperType, IIdentityProviderMapper> _byType = mappers.ToDictionary(m => m.MapperType);

    public IIdentityProviderMapper? Resolve(IdentityProviderMapperType mapperType)
        => _byType.GetValueOrDefault(mapperType);
}
