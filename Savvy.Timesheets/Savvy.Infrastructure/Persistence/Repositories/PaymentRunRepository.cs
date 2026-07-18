using Microsoft.EntityFrameworkCore;
using Savvy.Application.Persistence;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Repositories;

public sealed class PaymentRunRepository(TimesheetsDbContext dbContext)
    : Repository<PaymentRun>(dbContext),
        IPaymentRunRepository
{
    /// <summary>Finds a payment run by its idempotency business reference.</summary>
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

    /// <summary>Loads a payment run together with its line items and clinician details.</summary>
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
