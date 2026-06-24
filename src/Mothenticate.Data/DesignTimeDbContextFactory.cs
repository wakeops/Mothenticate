using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Mothenticate.Domain.Config;

namespace Mothenticate.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MothenticateDbContext>
{
    public MothenticateDbContext CreateDbContext(string[] args)
    {
        var configRoot = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "Mothenticate")
        );

        var configuration = new ConfigurationBuilder()
            .SetBasePath(configRoot)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var appConfig = configuration.Get<AppConfig>();

        var options = new DbContextOptionsBuilder<MothenticateDbContext>()
            .UseNpgsql(appConfig?.PostgreSQL.GetConnectionString())
            .Options;

        return new MothenticateDbContext(options);
    }
}
