
using Mothenticate.Data.Entities;

namespace Mothenticate.IdentityProvider.Services.ScopeMappers.Abstract;

public interface IScopeMapper
{
    MapperType MapperType { get; }
}