using Mothenticate.Domain.Config;
using Npgsql;

namespace Mothenticate.Data;

public static class ConfigExtensions
{
    public static string GetConnectionString(this AppConfigDatabase dbConfig)
    {
        if (dbConfig.Host == null)
        {
            return string.Empty;
        }

        var parts = dbConfig.Host.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = parts[0],
            Database = dbConfig.Name,
            Username = dbConfig.User,
            Password = dbConfig.Password,
            Pooling = true,
            MinPoolSize = 0,
            MaxPoolSize = 100
        };

        if (parts.Length == 2 && int.TryParse(parts[1], out var port))
        {
            builder.Port = port;
        }

        return builder.ConnectionString;
    }
}