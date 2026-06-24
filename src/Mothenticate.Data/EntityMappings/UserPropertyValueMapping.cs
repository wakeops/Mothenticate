using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class UserPropertyValueMapping : IEntityTypeConfiguration<UserPropertyValue>
{
    public void Configure(EntityTypeBuilder<UserPropertyValue> builder)
    {
        builder.HasKey(v => v.Id);

        builder.HasIndex(v => new { v.UserId, v.PropertyId }).IsUnique();

        builder.Property(v => v.UserId).HasMaxLength(450).IsRequired();
    }
}
