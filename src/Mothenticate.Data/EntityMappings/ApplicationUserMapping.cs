using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class ApplicationUserMapping : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(100);
        builder.Property(u => u.LastName).HasMaxLength(100);
        builder.Property(u => u.DisplayName).HasMaxLength(150);
        builder.Property(u => u.AvatarContentType).HasMaxLength(50);

        builder.HasMany(u => u.UserGroups)
            .WithOne(ug => ug.User)
            .HasForeignKey(ug => ug.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.PropertyValues)
            .WithOne(pv => pv.User)
            .HasForeignKey(pv => pv.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
