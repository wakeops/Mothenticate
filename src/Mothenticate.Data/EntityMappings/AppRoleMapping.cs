using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class AppRoleMapping : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(1000);

        builder.HasIndex(r => r.Name).IsUnique();

        builder.HasMany(r => r.GroupRoles)
            .WithOne(gr => gr.Role)
            .HasForeignKey(gr => gr.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
