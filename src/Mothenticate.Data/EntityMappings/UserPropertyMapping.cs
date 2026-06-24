using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class UserPropertyMapping : IEntityTypeConfiguration<UserProperty>
{
    public void Configure(EntityTypeBuilder<UserProperty> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Type).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(p => p.Name).IsUnique();

        builder.HasMany(p => p.Values)
            .WithOne(v => v.Property)
            .HasForeignKey(v => v.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
