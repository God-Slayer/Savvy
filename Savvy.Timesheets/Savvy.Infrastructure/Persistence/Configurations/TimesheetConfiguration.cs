using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Configurations;

internal sealed class TimesheetConfiguration : EntityConfigurationBase<Timesheet>
{
    public override void Configure(EntityTypeBuilder<Timesheet> builder)
    {
        base.Configure(builder);
        builder.Property(entity => entity.BusinessReference).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.RequestHash).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Notes).HasMaxLength(1000);
        builder
            .Property(entity => entity.Status)
            .HasConversion<int>()
            .HasDefaultValue(TimesheetStatus.Submitted);
        builder.HasIndex(entity => entity.BusinessReference).IsUnique();
        builder.HasIndex(entity => entity.ShiftId).IsUnique();
        builder
            .HasOne(entity => entity.Shift)
            .WithOne(entity => entity.Timesheet)
            .HasForeignKey<Timesheet>(entity => entity.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
