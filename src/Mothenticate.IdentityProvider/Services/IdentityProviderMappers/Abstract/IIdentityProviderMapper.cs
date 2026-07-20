using Mothenticate.Data.Entities;

namespace Mothenticate.IdentityProvider.Services.IdentityProviderMappers.Abstract;

public interface IIdentityProviderMapper
{
    IdentityProviderMapperType MapperType { get; }
}
