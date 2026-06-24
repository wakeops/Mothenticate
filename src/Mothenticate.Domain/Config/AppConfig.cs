using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace Mothenticate.Domain.Config;

public class AppConfig
{
    [Required(ErrorMessage = "Required parameter ISSUER_URI is not configured.")]
    [ConfigurationKeyName("issuer_uri")]
    public string? IssuerUri { get; set; }

    public AppConfigDatabase PostgreSQL { get; set; } = new AppConfigDatabase();
}

public class AppConfigDatabase
{
    [Required(ErrorMessage = "Required parameter POSTGRESQL__HOST is not configured.")]
    public string? Host { get; set; }

    [Required(ErrorMessage = "Required parameter POSTGRESQL__NAME is not configured.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Required parameter POSTGRESQL__USER is not configured.")]
    public string? User { get; set; }

    [Required(ErrorMessage = "Required parameter POSTGRESQL__PASSWORD is not configured.")]
    public string? Password { get; set; }
}