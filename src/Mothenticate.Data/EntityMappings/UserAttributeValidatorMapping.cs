using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mothenticate.Data.Entities;

namespace Mothenticate.Data.EntityMappings;

public class UserAttributeValidatorMapping : IEntityTypeConfiguration<UserAttributeValidator>
{
    public void Configure(EntityTypeBuilder<UserAttributeValidator> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.ValidatorType).HasConversion<string>().HasMaxLength(20);
        builder.Property(v => v.ConfigJson).HasColumnType("jsonb").IsRequired();
    }
}
