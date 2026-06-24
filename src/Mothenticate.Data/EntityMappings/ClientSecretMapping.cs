using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class ClientSecretMapping : IEntityTypeConfiguration<ClientSecret>
{
    public void Configure(EntityTypeBuilder<ClientSecret> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ApplicationId).HasMaxLength(450).IsRequired();
        builder.Property(s => s.SecretHash).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);

        builder.HasIndex(s => s.ApplicationId);
        builder.HasIndex(s => s.ExpiresAt);
    }
}
