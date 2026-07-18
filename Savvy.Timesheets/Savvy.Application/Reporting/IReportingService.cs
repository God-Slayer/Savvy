namespace Savvy.Application.Reporting;

public sealed record ReportSummary(
    Guid PracticeId,
    DateOnly From,
    DateOnly To,
    object[] Timesheets,
    object[] PaymentRuns
);

public interface IReportingService
{
    Task<ReportSummary> GetSummaryAsync(
        Guid practiceId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    );
}
