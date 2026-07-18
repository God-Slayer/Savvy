using Savvy.Domain;

namespace Savvy.Application.Persistence;

public interface IPaymentRunRepository : IRepository<PaymentRun>
{
    Task<PaymentRun?> GetByBusinessReferenceAsync(string businessReference, CancellationToken cancellationToken = default);
    Task<PaymentRun?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default);
}
