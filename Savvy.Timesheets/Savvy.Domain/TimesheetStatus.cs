namespace Savvy.Domain;

/// <summary>Defines the lifecycle of a submitted timesheet.</summary>
public enum TimesheetStatus
{
    Submitted = 1,
    Approved = 2,
    Paid = 3,
}
