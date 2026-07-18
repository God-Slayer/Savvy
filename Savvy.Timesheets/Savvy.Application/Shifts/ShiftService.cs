using Mapster;
using Savvy.Application.Common;
using Savvy.Application.Persistence;
using Savvy.Domain;

namespace Savvy.Application.Shifts;

/// <summary>Applies authorization and business rules for shift operations.</summary>
public sealed class ShiftService(IUnitOfWork uow) : IShiftService
{
    private static ShiftResponseDto Map(Shift shift)
    {
        return shift.Adapt<ShiftResponseDto>();
    }

    private static Result<T> Deny<T>()
    {
        return Result<T>.Failure("forbidden", "You do not have access to this practice or shift.");
    }

    private static bool Manager(CallerContext caller, Guid practiceId)
    {
        return caller.Role == UserRole.Admin
            || (caller.Role == UserRole.PracticeManager && caller.PracticeId == practiceId);
    }

    /// <summary>Lists shifts visible to the authenticated caller.</summary>
    public async Task<Result<IReadOnlyList<ShiftResponseDto>>> ListAsync(
        Guid practiceId,
        CallerContext caller,
        string? status = null,
        CancellationToken ct = default
    )
    {
        if (caller.Role == UserRole.Clinician)
        {
            var shifts = await uow.Shifts.ListByClinicianAsync(caller.UserId, ct);
            return Result<IReadOnlyList<ShiftResponseDto>>.Success(
                shifts.Where(x => x.PracticeId == practiceId).Select(Map).ToList()
            );
        }
        if (!Manager(caller, practiceId))
            return Deny<IReadOnlyList<ShiftResponseDto>>();
        ShiftStatus? parsedStatus = null;
        if (
            !string.IsNullOrWhiteSpace(status)
            && !status.Equals("all", StringComparison.OrdinalIgnoreCase)
            && (!Enum.TryParse(status, true, out ShiftStatus value))
        )
            return Result<IReadOnlyList<ShiftResponseDto>>.Failure(
                "validation",
                "Invalid shift status. Use All, Open, or Completed."
            );
        if (
            !string.IsNullOrWhiteSpace(status)
            && !status.Equals("all", StringComparison.OrdinalIgnoreCase)
        )
            parsedStatus = Enum.Parse<ShiftStatus>(status, true);
        var practiceShifts = await uow.Shifts.ListByPracticeAsync(practiceId, parsedStatus, ct);
        return Result<IReadOnlyList<ShiftResponseDto>>.Success(practiceShifts.Select(Map).ToList());
    }

    /// <summary>Gets one shift after enforcing caller scope.</summary>
    public async Task<Result<ShiftResponseDto>> GetAsync(
        Guid id,
        CallerContext caller,
        CancellationToken ct = default
    )
    {
        var shift = await uow.Shifts.GetByIdWithDetailsAsync(id, ct);
        if (shift is null)
            return Result<ShiftResponseDto>.Failure("not_found", "Shift not found.");
        if (
            caller.Role == UserRole.Clinician
                ? shift.ClinicianId != caller.UserId
                : !Manager(caller, shift.PracticeId)
        )
            return Deny<ShiftResponseDto>();
        return Result<ShiftResponseDto>.Success(Map(shift));
    }

    /// <summary>Creates an open shift for a practice.</summary>
    public async Task<Result<ShiftResponseDto>> CreateAsync(
        Guid practiceId,
        ShiftCreateDto dto,
        CallerContext caller,
        CancellationToken ct = default
    )
    {
        if (!Manager(caller, practiceId))
            return Deny<ShiftResponseDto>();
        if (
            dto.ClinicianId == Guid.Empty
            || dto.ScheduledEndUtc <= dto.ScheduledStartUtc
            || dto.HourlyRate < 0
            || string.IsNullOrWhiteSpace(dto.Role)
            || string.IsNullOrWhiteSpace(dto.Location)
        )
            return Result<ShiftResponseDto>.Failure("validation", "Invalid shift details.");
        var clinician = await uow.Users.GetByIdAsync(dto.ClinicianId, ct);
        if (
            clinician is null
            || clinician.PracticeId != practiceId
            || clinician.Role != UserRole.Clinician
        )
            return Result<ShiftResponseDto>.Failure(
                "validation",
                "Clinician is invalid for this practice."
            );
        var shift = new Shift
        {
            PracticeId = practiceId,
            ClinicianId = dto.ClinicianId,
            ScheduledStartUtc = dto.ScheduledStartUtc,
            ScheduledEndUtc = dto.ScheduledEndUtc,
            Role = dto.Role.Trim(),
            Location = dto.Location.Trim(),
            HourlyRate = dto.HourlyRate,
            Status = ShiftStatus.Open,
            RowVersion = Guid.NewGuid().ToByteArray(),
        };
        await uow.Shifts.AddAsync(shift, ct);
        await uow.SaveChangesAsync(ct);
        return Result<ShiftResponseDto>.Success(Map(shift));
    }

    /// <summary>Updates an open shift using optimistic concurrency.</summary>
    public async Task<Result<ShiftResponseDto>> UpdateAsync(
        Guid id,
        ShiftUpdateDto dto,
        CallerContext caller,
        CancellationToken ct = default
    )
    {
        var shift = await uow.Shifts.GetByIdAsync(id, ct);
        if (shift is null)
            return Result<ShiftResponseDto>.Failure("not_found", "Shift not found.");
        if (!Manager(caller, shift.PracticeId))
            return Deny<ShiftResponseDto>();
        if (shift.Status == ShiftStatus.Completed)
            return Result<ShiftResponseDto>.Failure(
                "conflict",
                "Completed shifts cannot be updated."
            );
        if (
            !TryVersion(dto.RowVersion, shift.RowVersion)
            || dto.ScheduledEndUtc <= dto.ScheduledStartUtc
            || dto.HourlyRate < 0
            || string.IsNullOrWhiteSpace(dto.Role)
            || string.IsNullOrWhiteSpace(dto.Location)
        )
            return Result<ShiftResponseDto>.Failure("validation", "Invalid shift details.");
        shift.ScheduledStartUtc = dto.ScheduledStartUtc;
        shift.ScheduledEndUtc = dto.ScheduledEndUtc;
        shift.Role = dto.Role.Trim();
        shift.Location = dto.Location.Trim();
        shift.HourlyRate = dto.HourlyRate;
        shift.UpdatedAtUtc = DateTime.UtcNow;
        uow.Shifts.Update(shift);
        await uow.SaveChangesAsync(ct);
        return Result<ShiftResponseDto>.Success(Map(shift));
    }

    /// <summary>Soft-deletes an open shift.</summary>
    public async Task<Result<bool>> DeleteAsync(
        Guid id,
        CallerContext caller,
        CancellationToken ct = default
    )
    {
        var shift = await uow.Shifts.GetByIdAsync(id, ct);
        if (shift is null)
            return Result<bool>.Failure("not_found", "Shift not found.");
        if (!Manager(caller, shift.PracticeId))
            return Deny<bool>();
        if (shift.Status == ShiftStatus.Completed)
            return Result<bool>.Failure("conflict", "Completed shifts cannot be deleted.");
        uow.Shifts.SoftDelete(shift);
        await uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }

    private static bool TryVersion(string value, byte[] bytes)
    {
        try
        {
            return Convert.FromBase64String(value).SequenceEqual(bytes);
        }
        catch
        {
            return false;
        }
    }
}
