using Mothenticate.Data.Entities;
using Mothenticate.IdentityProvider.Services.IdentityProviderMappers.Abstract;

namespace Mothenticate.IdentityProvider.Services.IdentityProviderMappers;

public interface IIdentityProviderMapperResolver
{
    IIdentityProviderMapper? Resolve(IdentityProviderMapperType mapperType);
}
