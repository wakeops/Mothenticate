using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class GroupRoleMapping : IEntityTypeConfiguration<GroupRole>
{
    public void Configure(EntityTypeBuilder<GroupRole> builder)
    {
        builder.HasKey(gr => new { gr.GroupId, gr.RoleId });
    }
}
