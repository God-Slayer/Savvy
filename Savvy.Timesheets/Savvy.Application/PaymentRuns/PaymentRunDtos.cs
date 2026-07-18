namespace Savvy.Application.PaymentRuns;

public record PaymentRunCreateDto(
    string BusinessReference,
    DateOnly PeriodStartDate,
    DateOnly PeriodEndDate,
    decimal PercentageFeeRate,
    decimal FixedFeeAmount,
    string? Currency
);

public record PaymentRunResponseDto(
    Guid Id,
    string BusinessReference,
    string Status,
    DateOnly PeriodStartDate,
    DateOnly PeriodEndDate,
    decimal TotalGrossAmount,
    decimal TotalFeeAmount,
    decimal TotalNetAmount,
    string Currency,
    int LineCount,
    IReadOnlyList<PaymentRunLineResponseDto>? Lines = null
);

public sealed record PaymentRunLineResponseDto(
    Guid TimesheetId,
    Guid ClinicianId,
    decimal HoursWorked,
    decimal HourlyRate,
    decimal GrossAmount,
    decimal PercentageFeeAmount,
    decimal FixedFeeAmount,
    decimal TotalFeeAmount,
    decimal NetAmount,
    string? ClinicianName = null
);
