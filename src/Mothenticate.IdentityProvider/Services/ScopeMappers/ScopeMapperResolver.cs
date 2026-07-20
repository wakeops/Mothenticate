using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers;

public class ScopeMapperResolver(IEnumerable<IScopeMapper> mappers) : IScopeMapperResolver
{
    private readonly Dictionary<MapperType, IScopeMapper> _byType = mappers.ToDictionary(m => m.MapperType);

    public IScopeMapper? Resolve(MapperType mapperType)
        => _byType.GetValueOrDefault(mapperType);
}
