using Mothenticate.Data.Entities;

namespace Mothenticate.Data.Repositories;

public class AppSettingsRepository(MothenticateDbContext db) : IAppSettingsRepository
{
    private const int SingletonId = 1;

    public async Task<AppSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await db.AppSettings.FindAsync([SingletonId], cancellationToken);

        if (settings is null)
        {
            settings = new AppSettings { Id = SingletonId };
            db.AppSettings.Add(settings);
            await db.SaveChangesAsync(cancellationToken);
        }

        if (string.IsNullOrEmpty(settings.DefaultLanguage))
            settings.DefaultLanguage = "en";

        return settings;
    }

    public async Task UpdateAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        db.AppSettings.Update(settings);
        await db.SaveChangesAsync(cancellationToken);
    }
}
