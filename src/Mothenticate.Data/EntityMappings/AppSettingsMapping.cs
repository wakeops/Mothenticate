using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class AppSettingsMapping : IEntityTypeConfiguration<AppSettings>
{
    public void Configure(EntityTypeBuilder<AppSettings> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.BrandingTitle).HasMaxLength(200);
        builder.Property(s => s.BrandingSubtitle).HasMaxLength(500);
        builder.Property(s => s.GoogleClientId).HasMaxLength(300);
        builder.Property(s => s.GoogleClientSecret).HasMaxLength(300);
        builder.Property(s => s.GitHubClientId).HasMaxLength(300);
        builder.Property(s => s.GitHubClientSecret).HasMaxLength(300);
    }
}
