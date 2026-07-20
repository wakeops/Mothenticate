using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class IdentityProviderMapperMapping : IEntityTypeConfiguration<IdentityProviderMapper>
{
    public void Configure(EntityTypeBuilder<IdentityProviderMapper> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
        builder.Property(m => m.SyncMode).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.MapperType).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.Config).HasColumnType("jsonb").IsRequired();

        builder.HasIndex(m => new { m.IdentityProviderId, m.Name }).IsUnique();
    }
}
