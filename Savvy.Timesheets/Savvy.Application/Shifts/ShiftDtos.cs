namespace Savvy.Application.Shifts;

public sealed record ShiftCreateDto(
    Guid ClinicianId,
    DateTime ScheduledStartUtc,
    DateTime ScheduledEndUtc,
    string Role,
    string Location,
    decimal HourlyRate
);

public sealed record ShiftUpdateDto(
    DateTime ScheduledStartUtc,
    DateTime ScheduledEndUtc,
    string Role,
    string Location,
    decimal HourlyRate,
    string RowVersion
);

public sealed record ShiftResponseDto(
    Guid Id,
    Guid PracticeId,
    Guid ClinicianId,
    DateTime ScheduledStartUtc,
    DateTime ScheduledEndUtc,
    string Role,
    string Location,
    decimal HourlyRate,
    string Status,
    string RowVersion
);
