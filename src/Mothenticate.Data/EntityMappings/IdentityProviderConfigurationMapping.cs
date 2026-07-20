using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class IdentityProviderConfigurationMapping : IEntityTypeConfiguration<IdentityProviderConfiguration>
{
    public void Configure(EntityTypeBuilder<IdentityProviderConfiguration> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Properties)
            .IsRequired()
            .HasDefaultValue("{}")
            .HasColumnType("jsonb");
    }
}
