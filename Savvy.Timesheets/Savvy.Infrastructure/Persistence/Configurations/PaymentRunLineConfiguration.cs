using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Configurations;

internal sealed class PaymentRunLineConfiguration : EntityConfigurationBase<PaymentRunLine>
{
    public override void Configure(EntityTypeBuilder<PaymentRunLine> builder)
    {
        base.Configure(builder);
        builder.HasIndex(entity => entity.TimesheetId).IsUnique();
        builder.Property(entity => entity.HoursWorked).HasPrecision(18, 2);
        builder.Property(entity => entity.HourlyRate).HasPrecision(18, 2);
        builder.Property(entity => entity.GrossAmount).HasPrecision(18, 2);
        builder.Property(entity => entity.PercentageFeeAmount).HasPrecision(18, 2);
        builder.Property(entity => entity.FixedFeeAmount).HasPrecision(18, 2);
        builder.Property(entity => entity.TotalFeeAmount).HasPrecision(18, 2);
        builder.Property(entity => entity.NetAmount).HasPrecision(18, 2);
        builder
            .HasOne(entity => entity.PaymentRun)
            .WithMany(entity => entity.Lines)
            .HasForeignKey(entity => entity.PaymentRunId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(entity => entity.Timesheet)
            .WithOne(entity => entity.PaymentRunLine)
            .HasForeignKey<PaymentRunLine>(entity => entity.TimesheetId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(entity => entity.Shift)
            .WithMany()
            .HasForeignKey(entity => entity.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(entity => entity.Clinician)
            .WithMany(entity => entity.PaymentRunLines)
            .HasForeignKey(entity => entity.ClinicianId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
