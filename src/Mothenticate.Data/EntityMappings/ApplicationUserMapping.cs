using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class ApplicationUserMapping : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // UserName, NormalizedUserName, and Email/NormalizedEmail live exclusively in UserAttributeValue
        // (see ApplicationUserStore / ApplicationUserHydrator) — not mapped columns.
        builder.Ignore(u => u.UserName);
        builder.Ignore(u => u.NormalizedUserName);
        builder.Ignore(u => u.Email);
        builder.Ignore(u => u.NormalizedEmail);

        builder.HasMany(u => u.UserGroups)
            .WithOne(ug => ug.User)
            .HasForeignKey(ug => ug.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.AttributeValues)
            .WithOne(v => v.User)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
