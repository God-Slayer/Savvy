using Savvy.Application.Common;
using Savvy.Application.Persistence;
using Savvy.Application.Shifts;
using Savvy.Domain;

namespace Savvy.Application.Timesheets;

public sealed class TimesheetService(
    ITimesheetRepository timesheets,
    IShiftRepository shifts,
    IUnitOfWork uow
) : ITimesheetService
{
    public async Task<Result<TimesheetResponseDto>> SubmitAsync(
        Guid shiftId,
        TimesheetSubmitDto dto,
        CallerContext c,
        CancellationToken ct = default
    )
    {
        var shift = await shifts.GetByIdAsync(shiftId, ct);
        if (shift is null || shift.IsDeleted)
            return Result<TimesheetResponseDto>.Failure("not_found", "Shift not found.");
        if (c.Role == UserRole.Clinician && shift.ClinicianId != c.UserId)
            return Result<TimesheetResponseDto>.Failure("forbidden", "Not assigned to this shift.");
        if (c.Role == UserRole.PracticeManager && c.PracticeId != shift.PracticeId)
            return Result<TimesheetResponseDto>.Failure("forbidden", "Outside practice scope.");
        if (c.Role is not (UserRole.Admin or UserRole.PracticeManager or UserRole.Clinician))
            return Result<TimesheetResponseDto>.Failure("forbidden", "Insufficient role.");
        var elapsedMinutes = (dto.ActualEndUtc - dto.ActualStartUtc).TotalMinutes;
        if (
            string.IsNullOrWhiteSpace(dto.BusinessReference)
            || dto.ActualEndUtc <= dto.ActualStartUtc
            || dto.UnpaidBreakMinutes is < 0
            || dto.UnpaidBreakMinutes >= elapsedMinutes
        )
            return Result<TimesheetResponseDto>.Failure("validation", "Invalid timesheet values.");
        if (
            dto.ActualStartUtc < shift.ScheduledStartUtc.AddHours(-12)
            || dto.ActualEndUtc > shift.ScheduledEndUtc.AddHours(12)
        )
            return Result<TimesheetResponseDto>.Failure(
                "validation",
                "Actual times fall outside allowable shift window."
            );
        var existing = await timesheets.GetByBusinessReferenceAsync(dto.BusinessReference, ct);
        if (existing is not null)
            return Result<TimesheetResponseDto>.Success(ToDto(existing, shift));
        if (shift.Status == ShiftStatus.Completed)
            return Result<TimesheetResponseDto>.Failure(
                "conflict",
                "Completed shifts cannot be changed."
            );
        if (await timesheets.GetByShiftIdAsync(shiftId, ct) is not null)
            return Result<TimesheetResponseDto>.Failure(
                "conflict",
                "A timesheet already exists for this shift."
            );
        var t = new Timesheet
        {
            ShiftId = shiftId,
            BusinessReference = dto.BusinessReference.Trim(),
            RequestHash = dto.BusinessReference.Trim(),
            ActualStartUtc = dto.ActualStartUtc,
            ActualEndUtc = dto.ActualEndUtc,
            UnpaidBreakMinutes = dto.UnpaidBreakMinutes,
            Notes = dto.Notes,
        };
        await timesheets.AddAsync(t, ct);
        shift.Status = ShiftStatus.Completed;
        shifts.Update(shift);
        await uow.SaveChangesAsync(ct);
        return Result<TimesheetResponseDto>.Success(ToDto(t, shift));
    }

    public async Task<Result<TimesheetResponseDto>> GetAsync(
        Guid id,
        CallerContext c,
        CancellationToken ct = default
    )
    {
        var t = await timesheets.GetByIdAsync(id, ct);
        if (t is null)
            return Result<TimesheetResponseDto>.Failure("not_found", "Timesheet not found.");
        var s = await shifts.GetByIdAsync(t.ShiftId, ct);
        if (s is null)
            return Result<TimesheetResponseDto>.Failure("not_found", "Shift not found.");
        if (c.Role == UserRole.Clinician && s.ClinicianId != c.UserId)
            return Result<TimesheetResponseDto>.Failure("forbidden", "Forbidden.");
        if (c.Role == UserRole.PracticeManager && c.PracticeId != s.PracticeId)
            return Result<TimesheetResponseDto>.Failure("forbidden", "Forbidden.");
        return Result<TimesheetResponseDto>.Success(ToDto(t, s));
    }

    public async Task<Result<TimesheetResponseDto>> ApproveAsync(
        Guid id,
        CallerContext c,
        CancellationToken ct = default
    )
    {
        if (c.Role is not (UserRole.Admin or UserRole.PracticeManager))
            return Result<TimesheetResponseDto>.Failure(
                "forbidden",
                "Only managers or admins may approve."
            );
        var t = await timesheets.GetByIdAsync(id, ct);
        if (t is null)
            return Result<TimesheetResponseDto>.Failure("not_found", "Timesheet not found.");
        var s = await shifts.GetByIdAsync(t.ShiftId, ct);
        if (s is null)
            return Result<TimesheetResponseDto>.Failure("not_found", "Shift not found.");
        if (c.Role == UserRole.PracticeManager && c.PracticeId != s.PracticeId)
            return Result<TimesheetResponseDto>.Failure("forbidden", "Forbidden.");
        if (t.Status == TimesheetStatus.Paid)
            return Result<TimesheetResponseDto>.Failure(
                "conflict",
                "Paid timesheets cannot change."
            );
        t.Status = TimesheetStatus.Approved;
        s.Status = ShiftStatus.Completed;
        timesheets.Update(t);
        shifts.Update(s);
        await uow.SaveChangesAsync(ct);
        return Result<TimesheetResponseDto>.Success(ToDto(t, s));
    }

    static TimesheetResponseDto ToDto(Timesheet t, Shift s)
    {
        var h = CalculationHelper.HoursWorked(
            t.ActualStartUtc,
            t.ActualEndUtc,
            t.UnpaidBreakMinutes
        );
        return new(
            t.Id,
            t.ShiftId,
            t.BusinessReference,
            t.ActualStartUtc,
            t.ActualEndUtc,
            t.UnpaidBreakMinutes,
            h,
            t.Notes,
            t.Status.ToString(),
            Convert.ToBase64String(t.RowVersion ?? [])
        );
    }
}
