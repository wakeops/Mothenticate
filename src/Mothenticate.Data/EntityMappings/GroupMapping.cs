using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class GroupMapping : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name).HasMaxLength(200).IsRequired();
        builder.Property(g => g.Description).HasMaxLength(1000);

        builder.HasIndex(g => g.Name).IsUnique();

        builder.HasMany(g => g.UserGroups)
            .WithOne(ug => ug.Group)
            .HasForeignKey(ug => ug.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.GroupRoles)
            .WithOne(gr => gr.Group)
            .HasForeignKey(gr => gr.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
