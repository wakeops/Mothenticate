namespace Mothenticate.Data.Services;

public interface ISeedService
{
    Task SeedAsync(CancellationToken cancellationToken = default);
    Task<bool> IsFirstRunAsync(CancellationToken cancellationToken = default);
}
