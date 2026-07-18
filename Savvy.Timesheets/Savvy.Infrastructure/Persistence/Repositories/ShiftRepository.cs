using Microsoft.EntityFrameworkCore;
using Savvy.Application.Persistence;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Repositories;

public sealed class ShiftRepository(TimesheetsDbContext dbContext)
    : Repository<Shift>(dbContext),
        IShiftRepository
{
    public async Task<IReadOnlyList<Shift>> ListByPracticeAsync(
        Guid practiceId,
        CancellationToken cancellationToken = default
    )
    {
        return await Entities
            .AsNoTracking()
            .Include(x => x.Clinician)
            .Where(x => x.PracticeId == practiceId)
            .OrderBy(x => x.ScheduledStartUtc)
            .ToListAsync(cancellationToken);
    }

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
