using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class AuditLogMapping : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ActorUserId).HasMaxLength(450);
        builder.Property(a => a.ResourceType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.ResourceId).HasMaxLength(450).IsRequired();
        builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(a => new { a.ResourceType, a.ResourceId });
        builder.HasIndex(a => a.ActorUserId);
        builder.HasIndex(a => a.Timestamp);
    }
}
