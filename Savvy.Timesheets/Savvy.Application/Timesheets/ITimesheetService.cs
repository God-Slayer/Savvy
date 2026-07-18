using Savvy.Application.Common;
using Savvy.Application.Shifts;

namespace Savvy.Application.Timesheets;

public interface ITimesheetService
{
    Task<Result<TimesheetResponseDto>> SubmitAsync(
        Guid shiftId,
        TimesheetSubmitDto dto,
        CallerContext caller,
        CancellationToken ct = default
    );
    Task<Result<TimesheetResponseDto>> GetAsync(
        Guid id,
        CallerContext caller,
        CancellationToken ct = default
    );
    Task<Result<TimesheetResponseDto>> ApproveAsync(
        Guid id,
        CallerContext caller,
        CancellationToken ct = default
    );
}
