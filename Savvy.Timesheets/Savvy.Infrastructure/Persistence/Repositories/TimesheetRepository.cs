using Microsoft.EntityFrameworkCore;
using Savvy.Application.Persistence;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Repositories;

public sealed class TimesheetRepository(TimesheetsDbContext dbContext)
    : Repository<Timesheet>(dbContext),
        ITimesheetRepository
{
    public Task<Timesheet?> GetByBusinessReferenceAsync(
        string businessReference,
        CancellationToken cancellationToken = default
    )
    {
        return Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                timesheet => timesheet.BusinessReference == businessReference,
                cancellationToken
            );
    }

    public Task<Timesheet?> GetByShiftIdAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default
    )
    {
        return Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(timesheet => timesheet.ShiftId == shiftId, cancellationToken);
    }

    public async Task<IReadOnlyList<Timesheet>> GetApprovedForPaymentAsync(
        Guid practiceId,
        DateOnly start,
        DateOnly end,
        CancellationToken ct = default
    )
    {
        return await Entities
            .Include(t => t.Shift)
            .AsNoTracking()
            .Where(t =>
                t.Status == TimesheetStatus.Approved
                && t.Shift!.PracticeId == practiceId
                && DateOnly.FromDateTime(t.ActualStartUtc) >= start
                && DateOnly.FromDateTime(t.ActualEndUtc) <= end
                && t.PaymentRunLine == null
            )
            .ToListAsync(ct);
    }
}
