using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class AppLauncherMapping : IEntityTypeConfiguration<AppLauncher>
{
    public void Configure(EntityTypeBuilder<AppLauncher> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Slug).IsRequired().HasMaxLength(100);
        builder.HasIndex(a => a.Slug).IsUnique();
        builder.Property(a => a.Description).HasMaxLength(1000);
        builder.Property(a => a.IconUrl).HasMaxLength(500);
        builder.Property(a => a.LaunchUrl).HasMaxLength(500);
        builder.Property(a => a.LinkedClientId).HasMaxLength(450);
    }
}
