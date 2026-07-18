using Savvy.Domain;

namespace Savvy.Application.Persistence;

public interface ITimesheetRepository : IRepository<Timesheet>
{
    Task<IReadOnlyList<Timesheet>> GetByPracticeAsync(
        Guid practiceId,
        CancellationToken cancellationToken = default
    );
    Task<Timesheet?> GetByBusinessReferenceAsync(
        string businessReference,
        CancellationToken cancellationToken = default
    );
    Task<Timesheet?> GetByShiftIdAsync(Guid shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Timesheet>> GetByPracticeAsync(
        Guid practiceId,
        TimesheetStatus? status,
        CancellationToken cancellationToken = default
    )
    {
        return FilterByStatusAsync(practiceId, status, cancellationToken);
    }

    Task<IReadOnlyList<Timesheet>> GetByClinicianAsync(
        Guid clinicianId,
        TimesheetStatus? status,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult<IReadOnlyList<Timesheet>>(Array.Empty<Timesheet>());
    }

    private async Task<IReadOnlyList<Timesheet>> FilterByStatusAsync(
        Guid practiceId,
        TimesheetStatus? status,
        CancellationToken cancellationToken
    )
    {
        var records = await GetByPracticeAsync(practiceId, cancellationToken);
        return status is null ? records : records.Where(x => x.Status == status).ToList();
    }
    Task<IReadOnlyList<Timesheet>> GetApprovedForPaymentAsync(
        Guid practiceId,
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default
    );
}
