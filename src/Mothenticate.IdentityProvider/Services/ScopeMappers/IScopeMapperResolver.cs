using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers;

public interface IScopeMapperResolver
{
    IScopeMapper? Resolve(MapperType mapperType);
}
