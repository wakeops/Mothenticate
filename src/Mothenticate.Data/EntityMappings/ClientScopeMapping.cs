using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class ClientScopeMapping : IEntityTypeConfiguration<ClientScope>
{
    public void Configure(EntityTypeBuilder<ClientScope> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.ConsentText).HasMaxLength(500);
        builder.Property(s => s.AssignedType).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Protocol).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(s => s.Name).IsUnique();

        builder.HasMany(s => s.Mappers)
            .WithOne(m => m.ClientScope)
            .HasForeignKey(m => m.ClientScopeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
