using Savvy.Domain;

namespace Savvy.Application.Persistence;

public interface IShiftRepository : IRepository<Shift>
{
    Task<IReadOnlyList<Shift>> ListByPracticeAsync(
        Guid practiceId,
        CancellationToken cancellationToken = default
    );
    async Task<IReadOnlyList<Shift>> ListByPracticeAsync(
        Guid practiceId,
        ShiftStatus? status,
        CancellationToken cancellationToken = default
    )
    {
        var shifts = await ListByPracticeAsync(practiceId, cancellationToken);
        return status is null ? shifts : shifts.Where(x => x.Status == status).ToList();
    }
    Task<IReadOnlyList<Shift>> ListByClinicianAsync(
        Guid clinicianId,
        CancellationToken cancellationToken = default
    );
    Task<Shift?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
