using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Configurations;

internal sealed class ShiftConfiguration : EntityConfigurationBase<Shift>
{
    public override void Configure(EntityTypeBuilder<Shift> builder)
    {
        base.Configure(builder);
        builder.Property(entity => entity.Role).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.Location).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.HourlyRate).HasPrecision(18, 2);
        builder
            .Property(entity => entity.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder
            .HasOne(entity => entity.Practice)
            .WithMany(entity => entity.Shifts)
            .HasForeignKey(entity => entity.PracticeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(entity => entity.Clinician)
            .WithMany(entity => entity.AssignedShifts)
            .HasForeignKey(entity => entity.ClinicianId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
