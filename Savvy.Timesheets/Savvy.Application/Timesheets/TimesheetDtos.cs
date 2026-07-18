namespace Savvy.Application.Timesheets;

public sealed record TimesheetSubmitDto(
    string BusinessReference,
    DateTime ActualStartUtc,
    DateTime ActualEndUtc,
    int? UnpaidBreakMinutes,
    string? Notes
);

public sealed record TimesheetResponseDto(
    Guid Id,
    Guid ShiftId,
    string BusinessReference,
    DateTime ActualStartUtc,
    DateTime ActualEndUtc,
    int? UnpaidBreakMinutes,
    decimal HoursWorked,
    string? Notes,
    string Status,
    string RowVersion
);
