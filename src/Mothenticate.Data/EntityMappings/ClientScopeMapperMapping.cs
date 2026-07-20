using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class ClientScopeMapperMapping : IEntityTypeConfiguration<ClientScopeMapper>
{
    public void Configure(EntityTypeBuilder<ClientScopeMapper> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
        builder.Property(m => m.MapperType).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.Config).HasColumnType("jsonb").IsRequired();

        builder.HasIndex(m => new { m.ClientScopeId, m.Name }).IsUnique();
    }
}
