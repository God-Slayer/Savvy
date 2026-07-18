using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : EntityConfigurationBase<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        builder.Property(entity => entity.Email).HasMaxLength(320).IsRequired();
        builder.Property<string>("NormalizedEmail").HasMaxLength(320).IsRequired();
        builder.HasIndex("NormalizedEmail").IsUnique();
        builder.Property(entity => entity.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(entity => entity.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.LastName).HasMaxLength(100).IsRequired();
        builder
            .Property(entity => entity.Role)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder
            .HasOne(entity => entity.Practice)
            .WithMany(entity => entity.Users)
            .HasForeignKey(entity => entity.PracticeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
