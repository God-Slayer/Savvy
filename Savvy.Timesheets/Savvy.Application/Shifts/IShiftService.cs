using Savvy.Application.Common;

namespace Savvy.Application.Shifts;

public interface IShiftService
{
    Task<Result<IReadOnlyList<ShiftResponseDto>>> ListAsync(
        Guid practiceId,
        CallerContext caller,
        string? status = null,
        CancellationToken ct = default
    );
    Task<Result<ShiftResponseDto>> GetAsync(
        Guid id,
        CallerContext caller,
        CancellationToken ct = default
    );
    Task<Result<ShiftResponseDto>> CreateAsync(
        Guid practiceId,
        ShiftCreateDto dto,
        CallerContext caller,
        CancellationToken ct = default
    );
    Task<Result<ShiftResponseDto>> UpdateAsync(
        Guid id,
        ShiftUpdateDto dto,
        CallerContext caller,
        CancellationToken ct = default
    );
    Task<Result<bool>> DeleteAsync(Guid id, CallerContext caller, CancellationToken ct = default);
}
