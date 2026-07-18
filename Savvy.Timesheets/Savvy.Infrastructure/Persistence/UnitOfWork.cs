using Savvy.Application.Persistence;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence;

public sealed class UnitOfWork(
    TimesheetsDbContext dbContext,
    IRepository<Practice> practices,
    IUserRepository users,
    IShiftRepository shifts,
    ITimesheetRepository timesheets,
    IPaymentRunRepository paymentRuns
) : IUnitOfWork
{
    private readonly TimesheetsDbContext _dbContext = dbContext;

    public IRepository<Practice> Practices { get; } = practices;
    public IUserRepository Users { get; } = users;
    public IShiftRepository Shifts { get; } = shifts;
    public ITimesheetRepository Timesheets { get; } = timesheets;
    public IPaymentRunRepository PaymentRuns { get; } = paymentRuns;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
