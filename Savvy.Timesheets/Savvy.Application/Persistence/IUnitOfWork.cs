using Savvy.Domain;

namespace Savvy.Application.Persistence;

public interface IUnitOfWork
{
    IRepository<Practice> Practices { get; }
    IUserRepository Users { get; }
    IShiftRepository Shifts { get; }
    ITimesheetRepository Timesheets { get; }
    IPaymentRunRepository PaymentRuns { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
