using Microsoft.EntityFrameworkCore;
using Savvy.Application.Persistence;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Repositories;

public sealed class PaymentRunRepository(TimesheetsDbContext dbContext)
    : Repository<PaymentRun>(dbContext),
        IPaymentRunRepository
{
    public Task<PaymentRun?> GetByBusinessReferenceAsync(
        string businessReference,
        CancellationToken cancellationToken = default
    )
    {
        return Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                paymentRun => paymentRun.BusinessReference == businessReference,
                cancellationToken
            );
    }

    public Task<PaymentRun?> GetByIdWithLinesAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return Entities
            .AsNoTracking()
            .Include(paymentRun => paymentRun.Lines)
                .ThenInclude(line => line.Timesheet)
            .Include(paymentRun => paymentRun.Lines)
                .ThenInclude(line => line.Clinician)
            .FirstOrDefaultAsync(paymentRun => paymentRun.Id == id, cancellationToken);
    }
}
