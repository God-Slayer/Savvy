namespace Savvy.Domain;

/// <summary>Represents scheduled work assigned to one clinician at a practice.</summary>
public sealed class Shift : BaseEntity
{
    public Guid PracticeId { get; set; }

    public Guid ClinicianId { get; set; }

    public DateTime ScheduledStartUtc { get; set; }

    public DateTime ScheduledEndUtc { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public decimal HourlyRate { get; set; }

    public ShiftStatus Status { get; set; } = ShiftStatus.Open;

    public Practice? Practice { get; set; }

    public User? Clinician { get; set; }

    public Timesheet? Timesheet { get; set; }
}
