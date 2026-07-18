namespace Savvy.Domain;

/// <summary>Records the actual worked time submitted against exactly one shift.</summary>
public sealed class Timesheet : BaseEntity
{
    public Guid ShiftId { get; set; }

    public string BusinessReference { get; set; } = string.Empty;

    public string RequestHash { get; set; } = string.Empty;

    public DateTime ActualStartUtc { get; set; }

    public DateTime ActualEndUtc { get; set; }

    public int? UnpaidBreakMinutes { get; set; }

    public string? Notes { get; set; }
    public TimesheetStatus Status { get; set; } = TimesheetStatus.Submitted;

    public Shift? Shift { get; set; }

    public PaymentRunLine? PaymentRunLine { get; set; }
}
