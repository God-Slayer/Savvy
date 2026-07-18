namespace Savvy.Domain;

/// <summary>Captures the calculated gross, fee, and net amounts for one timesheet in a payment run.</summary>
public sealed class PaymentRunLine : BaseEntity
{
    public Guid PaymentRunId { get; set; }

    public Guid TimesheetId { get; set; }

    public Guid ShiftId { get; set; }

    public Guid ClinicianId { get; set; }

    public decimal HoursWorked { get; set; }

    public decimal HourlyRate { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal PercentageFeeAmount { get; set; }

    public decimal FixedFeeAmount { get; set; }

    public decimal TotalFeeAmount { get; set; }

    public decimal NetAmount { get; set; }

    public PaymentRun? PaymentRun { get; set; }

    public Timesheet? Timesheet { get; set; }

    public Shift? Shift { get; set; }

    public User? Clinician { get; set; }
}
