using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class UserAttributeScopeMapping : IEntityTypeConfiguration<UserAttributeScope>
{
    public void Configure(EntityTypeBuilder<UserAttributeScope> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Purpose).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(s => new { s.UserAttributeId, s.ScopeId, s.Purpose }).IsUnique();

        builder.HasOne(s => s.Scope)
            .WithMany(c => c.AttributeScopes)
            .HasForeignKey(s => s.ScopeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
