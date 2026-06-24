using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class AppLauncherAccessMapping : IEntityTypeConfiguration<AppLauncherAccess>
{
    public void Configure(EntityTypeBuilder<AppLauncherAccess> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AppLauncherId);
        builder.Property(a => a.UserId).HasMaxLength(450);

        builder.HasOne(a => a.AppLauncher)
               .WithMany(app => app.AccessEntries)
               .HasForeignKey(a => a.AppLauncherId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.User)
               .WithMany()
               .HasForeignKey(a => a.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired(false);

        builder.HasOne(a => a.Group)
               .WithMany()
               .HasForeignKey(a => a.GroupId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired(false);

        builder.HasIndex(a => new { a.AppLauncherId, a.UserId });
        builder.HasIndex(a => new { a.AppLauncherId, a.GroupId });
    }
}
