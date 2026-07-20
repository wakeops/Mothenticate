using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class UserAttributeValueMapping : IEntityTypeConfiguration<UserAttributeValue>
{
    public void Configure(EntityTypeBuilder<UserAttributeValue> builder)
    {
        builder.HasKey(v => v.Id);

        builder.HasIndex(v => new { v.UserId, v.UserAttributeId, v.Ordinal }).IsUnique();

        builder.Property(v => v.UserId).HasMaxLength(450).IsRequired();
    }
}
