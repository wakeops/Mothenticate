using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class IdentityProviderMapping : IEntityTypeConfiguration<IdentityProvider>
{
    public void Configure(EntityTypeBuilder<IdentityProvider> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Alias).IsRequired().HasMaxLength(64);
        builder.HasIndex(p => p.Alias).IsUnique();

        builder.Property(p => p.DisplayName).IsRequired();

        builder.Property(p => p.ShowInAccountConsole).IsRequired().HasConversion<string>();
        builder.Property(p => p.SyncMode).IsRequired().HasConversion<string>();

        builder.Property(p => p.IsEnabled).IsRequired().HasDefaultValue(true);
        builder.Property(p => p.AccountLinkingOnly).IsRequired().HasDefaultValue(false);
        builder.Property(p => p.HideOnLoginPage).IsRequired().HasDefaultValue(false);

        builder.HasOne(p => p.Configuration)
            .WithMany()
            .HasForeignKey(p => p.ConfigurationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Mappers)
            .WithOne(m => m.IdentityProvider)
            .HasForeignKey(m => m.IdentityProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
