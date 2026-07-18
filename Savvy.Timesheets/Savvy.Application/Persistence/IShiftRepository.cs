using Savvy.Domain;

namespace Savvy.Application.Persistence;

public interface IShiftRepository : IRepository<Shift>
{
    Task<IReadOnlyList<Shift>> ListByPracticeAsync(Guid practiceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Shift>> ListByClinicianAsync(Guid clinicianId, CancellationToken cancellationToken = default);
    Task<Shift?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
