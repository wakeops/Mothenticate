using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class UserAttributeMapping : IEntityTypeConfiguration<UserAttribute>
{
    public void Configure(EntityTypeBuilder<UserAttribute> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).HasMaxLength(100).IsRequired();
        builder.Property(a => a.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.InputType).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.EnabledWhen).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.RequiredFor).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.RequiredWhen).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.DefaultValue).HasMaxLength(2000);

        builder.HasIndex(a => a.Name).IsUnique();

        builder.HasMany(a => a.Values)
            .WithOne(v => v.UserAttribute)
            .HasForeignKey(v => v.UserAttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Scopes)
            .WithOne(s => s.UserAttribute)
            .HasForeignKey(s => s.UserAttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Validators)
            .WithOne(v => v.UserAttribute)
            .HasForeignKey(v => v.UserAttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
