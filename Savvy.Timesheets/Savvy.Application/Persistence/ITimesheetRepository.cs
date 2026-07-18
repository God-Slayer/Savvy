using Savvy.Domain;

namespace Savvy.Application.Persistence;

public interface ITimesheetRepository : IRepository<Timesheet>
{
    Task<Timesheet?> GetByBusinessReferenceAsync(
        string businessReference,
        CancellationToken cancellationToken = default
    );
    Task<Timesheet?> GetByShiftIdAsync(Guid shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Timesheet>> GetApprovedForPaymentAsync(
        Guid practiceId,
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default
    );
}
