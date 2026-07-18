using Savvy.Application.Common;
using Savvy.Application.Shifts;

namespace Savvy.Application.PaymentRuns;

public interface IPaymentRunService
{
    Task<Result<PaymentRunResponseDto>> CreateAsync(
        Guid practiceId,
        PaymentRunCreateDto dto,
        CallerContext c,
        CancellationToken ct = default
    );
    Task<Result<PaymentRunResponseDto>> GetAsync(
        Guid id,
        CallerContext c,
        CancellationToken ct = default
    );
    Task<Result<PaymentRunResponseDto>> ProcessAsync(
        Guid id,
        CallerContext c,
        CancellationToken ct = default
    );
}
