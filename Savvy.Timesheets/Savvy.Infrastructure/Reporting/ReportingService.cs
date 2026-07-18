using Microsoft.EntityFrameworkCore;
using Savvy.Application.Reporting;
using Savvy.Infrastructure.Persistence;

namespace Savvy.Infrastructure.Reporting;

public sealed class ReportingService(TimesheetsDbContext db) : IReportingService
{
    public async Task<ReportSummary> GetSummaryAsync(
        Guid id,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default
    )
    {
        var source = await db
            .Timesheets.AsNoTracking()
            .Include(x => x.Shift)
            .Where(x =>
                x.Shift!.PracticeId == id
                && DateOnly.FromDateTime(x.ActualStartUtc) >= from
                && DateOnly.FromDateTime(x.ActualStartUtc) <= to
            )
            .ToListAsync(ct);
        var ts = source
            .GroupBy(x => x.Status)
            .Select(g =>
                (object)
                    new
                    {
                        status = g.Key.ToString(),
                        count = g.Count(),
                        hours = g.Sum(x =>
                            Savvy.Application.Common.CalculationHelper.HoursWorked(
                                x.ActualStartUtc,
                                x.ActualEndUtc,
                                x.UnpaidBreakMinutes
                            )
                        ),
                    }
            )
            .ToArray();
        var pr = await db
            .PaymentRuns.AsNoTracking()
            .Where(x => x.PracticeId == id && x.PeriodStartDate <= to && x.PeriodEndDate >= from)
            .Select(x =>
                (object)
                    new
                    {
                        x.Id,
                        x.BusinessReference,
                        status = x.Status.ToString(),
                        x.TotalGrossAmount,
                        x.TotalFeeAmount,
                        x.TotalNetAmount,
                    }
            )
            .ToArrayAsync(ct);
        return new(id, from, to, ts, pr);
    }
}
