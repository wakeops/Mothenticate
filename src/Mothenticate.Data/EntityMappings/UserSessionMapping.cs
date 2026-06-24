using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class UserSessionMapping : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId).HasMaxLength(450).IsRequired();
        builder.Property(s => s.IpAddress).HasMaxLength(45);
        builder.Property(s => s.UserAgent).HasMaxLength(500);
        builder.Property(s => s.DeviceLabel).HasMaxLength(200);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.RevokedAt);
    }
}
