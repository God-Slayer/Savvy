namespace Savvy.Domain;

/// <summary>Represents a practice payment batch and its calculated totals.</summary>
public sealed class PaymentRun : BaseEntity
{
    public Guid PracticeId { get; set; }

    public string BusinessReference { get; set; } = string.Empty;

    public string RequestHash { get; set; } = string.Empty;

    public DateOnly PeriodStartDate { get; set; }

    public DateOnly PeriodEndDate { get; set; }

    public decimal PercentageFeeRate { get; set; }

    public decimal FixedFeeAmount { get; set; }

    public decimal TotalGrossAmount { get; set; }

    public decimal TotalFeeAmount { get; set; }

    public decimal TotalNetAmount { get; set; }
    public PaymentRunStatus Status { get; set; } = PaymentRunStatus.Draft;

    public string Currency { get; set; } = "GBP";

    public Practice? Practice { get; set; }

    public ICollection<PaymentRunLine> Lines { get; set; } = new List<PaymentRunLine>();
}
