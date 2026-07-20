using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class IdentityProviderTypeMapping : IEntityTypeConfiguration<IdentityProviderType>
{
    public void Configure(EntityTypeBuilder<IdentityProviderType> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(64);
        builder.HasIndex(t => t.Name).IsUnique();

        builder.Property(t => t.ProtocolType).IsRequired().HasConversion<string>();

        builder.HasOne(t => t.DefaultConfiguration)
            .WithMany()
            .HasForeignKey(t => t.DefaultConfigurationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Providers)
            .WithOne(p => p.ProviderType)
            .HasForeignKey(p => p.ProviderTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
