using Microsoft.EntityFrameworkCore;
using Savvy.Application.Persistence;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Repositories;

public sealed class TimesheetRepository(TimesheetsDbContext dbContext)
    : Repository<Timesheet>(dbContext),
        ITimesheetRepository
{
    public Task<IReadOnlyList<Timesheet>> GetByPracticeAsync(
        Guid practiceId,
        CancellationToken cancellationToken = default
    )
    {
        return GetByPracticeAsync(practiceId, null, cancellationToken);
    }

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

    public async Task<IReadOnlyList<Timesheet>> GetByPracticeAsync(
        Guid practiceId,
        TimesheetStatus? status = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Entities
            .Include(timesheet => timesheet.Shift)
            .AsNoTracking()
            .Where(timesheet =>
                timesheet.Shift != null
                && timesheet.Shift.PracticeId == practiceId
                && !timesheet.IsDeleted
                && (status == null || timesheet.Status == status)
            )
            .OrderByDescending(timesheet => timesheet.ActualStartUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Timesheet>> GetByClinicianAsync(
        Guid clinicianId,
        TimesheetStatus? status,
        CancellationToken cancellationToken = default
    )
    {
        return await Entities
            .Include(timesheet => timesheet.Shift)
            .AsNoTracking()
            .Where(timesheet =>
                timesheet.Shift != null
                && timesheet.Shift.ClinicianId == clinicianId
                && !timesheet.IsDeleted
                && (status == null || timesheet.Status == status)
            )
            .OrderByDescending(timesheet => timesheet.ActualStartUtc)
            .ToListAsync(cancellationToken);
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
