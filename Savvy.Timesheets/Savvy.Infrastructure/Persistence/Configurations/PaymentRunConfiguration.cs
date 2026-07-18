using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Configurations;

internal sealed class PaymentRunConfiguration : EntityConfigurationBase<PaymentRun>
{
    public override void Configure(EntityTypeBuilder<PaymentRun> builder)
    {
        base.Configure(builder);
        builder.Property(entity => entity.BusinessReference).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.RequestHash).HasMaxLength(128).IsRequired();
        builder.Property(entity => entity.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.HasIndex(entity => entity.BusinessReference).IsUnique();
        builder.Property(entity => entity.PercentageFeeRate).HasPrecision(18, 2);
        builder.Property(entity => entity.FixedFeeAmount).HasPrecision(18, 2);
        builder.Property(entity => entity.TotalGrossAmount).HasPrecision(18, 2);
        builder.Property(entity => entity.TotalFeeAmount).HasPrecision(18, 2);
        builder.Property(entity => entity.TotalNetAmount).HasPrecision(18, 2);
        builder
            .HasOne(entity => entity.Practice)
            .WithMany(entity => entity.PaymentRuns)
            .HasForeignKey(entity => entity.PracticeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
