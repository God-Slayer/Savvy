using Microsoft.EntityFrameworkCore;
using Savvy.Application.Persistence;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Repositories;

public sealed class ShiftRepository(TimesheetsDbContext dbContext)
    : Repository<Shift>(dbContext),
        IShiftRepository
{
    /// <summary>Returns all shifts for a practice when no status filter is requested.</summary>
    public Task<IReadOnlyList<Shift>> ListByPracticeAsync(
        Guid practiceId,
        CancellationToken cancellationToken = default
    )
    {
        return ListByPracticeAsync(practiceId, null, cancellationToken);
    }

    /// <summary>Queries a practice's shifts with an optional open/completed status filter.</summary>
    public async Task<IReadOnlyList<Shift>> ListByPracticeAsync(
        Guid practiceId,
        ShiftStatus? status = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Entities
            .AsNoTracking()
            .Include(x => x.Clinician)
            .Where(x => x.PracticeId == practiceId && (status == null || x.Status == status))
            .OrderBy(x => x.ScheduledStartUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Returns shifts assigned to a specific clinician in schedule order.</summary>
    public async Task<IReadOnlyList<Shift>> ListByClinicianAsync(
        Guid clinicianId,
        CancellationToken cancellationToken = default
    )
    {
        return await Entities
            .AsNoTracking()
            .Include(x => x.Practice)
            .Where(x => x.ClinicianId == clinicianId)
            .OrderBy(x => x.ScheduledStartUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Loads one shift together with its practice, clinician, and timesheet details.</summary>
    public Task<Shift?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return Entities
            .AsNoTracking()
            .Include(shift => shift.Practice)
            .Include(shift => shift.Clinician)
            .Include(shift => shift.Timesheet)
            .FirstOrDefaultAsync(shift => shift.Id == id, cancellationToken);
    }
}
