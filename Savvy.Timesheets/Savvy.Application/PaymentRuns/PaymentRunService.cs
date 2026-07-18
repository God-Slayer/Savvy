using Savvy.Application.Common;
using Savvy.Application.Persistence;
using Savvy.Application.Shifts;
using Savvy.Domain;

namespace Savvy.Application.PaymentRuns;

public sealed class PaymentRunService(
    IPaymentRunRepository runs,
    ITimesheetRepository sheets,
    IUnitOfWork uow
) : IPaymentRunService
{
    public async Task<Result<PaymentRunResponseDto>> CreateAsync(
        Guid practiceId,
        PaymentRunCreateDto d,
        CallerContext c,
        CancellationToken ct = default
    )
    {
        if (
            c.Role is not (UserRole.Admin or UserRole.PracticeManager)
            || c.Role == UserRole.PracticeManager && c.PracticeId != practiceId
        )
            return Result<PaymentRunResponseDto>.Failure("forbidden", "Insufficient scope.");
        if (
            string.IsNullOrWhiteSpace(d.BusinessReference)
            || d.PeriodEndDate < d.PeriodStartDate
            || d.PercentageFeeRate < 0
            || d.FixedFeeAmount < 0
        )
            return Result<PaymentRunResponseDto>.Failure(
                "validation",
                "Invalid payment run values."
            );
        var existing = await runs.GetByBusinessReferenceAsync(d.BusinessReference, ct);
        if (existing != null)
            return Result<PaymentRunResponseDto>.Success(ToDto(existing));
        var ts = await sheets.GetApprovedForPaymentAsync(
            practiceId,
            d.PeriodStartDate,
            d.PeriodEndDate,
            ct
        );
        if (ts.Count == 0)
            return Result<PaymentRunResponseDto>.Failure(
                "validation",
                "No approved timesheets available."
            );
        var run = new PaymentRun
        {
            PracticeId = practiceId,
            BusinessReference = d.BusinessReference.Trim(),
            RequestHash = d.BusinessReference.Trim(),
            PeriodStartDate = d.PeriodStartDate,
            PeriodEndDate = d.PeriodEndDate,
            PercentageFeeRate = d.PercentageFeeRate,
            FixedFeeAmount = d.FixedFeeAmount,
            Currency = string.IsNullOrWhiteSpace(d.Currency)
                ? "GBP"
                : d.Currency!.Trim().ToUpperInvariant(),
        };
        foreach (var t in ts)
        {
            var hours = CalculationHelper.HoursWorked(
                t.ActualStartUtc,
                t.ActualEndUtc,
                t.UnpaidBreakMinutes
            );
            var gross = CalculationHelper.Money(hours * t.Shift!.HourlyRate);
            var pct = CalculationHelper.Money(gross * d.PercentageFeeRate / 100m);
            var fixedFee = CalculationHelper.Money(d.FixedFeeAmount);
            run.Lines.Add(
                new PaymentRunLine
                {
                    TimesheetId = t.Id,
                    ShiftId = t.ShiftId,
                    ClinicianId = t.Shift.ClinicianId,
                    HoursWorked = hours,
                    HourlyRate = t.Shift.HourlyRate,
                    GrossAmount = gross,
                    PercentageFeeAmount = pct,
                    FixedFeeAmount = fixedFee,
                    TotalFeeAmount = CalculationHelper.Money(pct + fixedFee),
                    NetAmount = CalculationHelper.Money(gross - pct - fixedFee),
                }
            );
            t.Status = TimesheetStatus.Paid;
            sheets.Update(t);
        }
        run.TotalGrossAmount = CalculationHelper.Money(run.Lines.Sum(x => x.GrossAmount));
        run.TotalFeeAmount = CalculationHelper.Money(run.Lines.Sum(x => x.TotalFeeAmount));
        run.TotalNetAmount = CalculationHelper.Money(run.Lines.Sum(x => x.NetAmount));
        await runs.AddAsync(run, ct);
        await uow.SaveChangesAsync(ct);
        return Result<PaymentRunResponseDto>.Success(ToDto(run));
    }

    public async Task<Result<PaymentRunResponseDto>> GetAsync(
        Guid id,
        CallerContext c,
        CancellationToken ct = default
    )
    {
        var r = await runs.GetByIdWithLinesAsync(id, ct);
        if (r == null)
            return Result<PaymentRunResponseDto>.Failure("not_found", "Payment run not found.");
        if (
            c.Role == UserRole.PracticeManager && c.PracticeId != r.PracticeId
            || c.Role != UserRole.Admin && c.Role != UserRole.PracticeManager
        )
            return Result<PaymentRunResponseDto>.Failure("forbidden", "Forbidden.");
        return Result<PaymentRunResponseDto>.Success(ToDto(r));
    }

    public async Task<Result<PaymentRunResponseDto>> ProcessAsync(
        Guid id,
        CallerContext c,
        CancellationToken ct = default
    )
    {
        var r = await runs.GetByIdWithLinesAsync(id, ct);
        if (r == null)
            return Result<PaymentRunResponseDto>.Failure("not_found", "Payment run not found.");
        if (
            c.Role != UserRole.Admin
            && !(c.Role == UserRole.PracticeManager && c.PracticeId == r.PracticeId)
        )
            return Result<PaymentRunResponseDto>.Failure("forbidden", "Forbidden.");
        if (r.Status == PaymentRunStatus.Processed)
            return Result<PaymentRunResponseDto>.Success(ToDto(r));
        // Reload only the payment-run entity before updating so EF does not
        // attempt to update its already-persisted line and navigation graph.
        var runToUpdate = await runs.GetByIdAsync(id, ct);
        if (runToUpdate is null)
            return Result<PaymentRunResponseDto>.Failure("not_found", "Payment run not found.");
        runToUpdate.Status = PaymentRunStatus.Processed;
        runs.Update(runToUpdate);
        r.Status = PaymentRunStatus.Processed;
        await uow.SaveChangesAsync(ct);
        return Result<PaymentRunResponseDto>.Success(ToDto(r));
    }

    // Maps a payment run and its lines to the API response contract.
    private static PaymentRunResponseDto ToDto(PaymentRun r)
    {
        return new PaymentRunResponseDto(
            r.Id,
            r.BusinessReference,
            r.Status.ToString(),
            r.PeriodStartDate,
            r.PeriodEndDate,
            r.TotalGrossAmount,
            r.TotalFeeAmount,
            r.TotalNetAmount,
            r.Currency,
            r.Lines.Count,
            r.Lines.Select(x => new PaymentRunLineResponseDto(
                    x.TimesheetId,
                    x.ClinicianId,
                    x.HoursWorked,
                    x.HourlyRate,
                    x.GrossAmount,
                    x.PercentageFeeAmount,
                    x.FixedFeeAmount,
                    x.TotalFeeAmount,
                    x.NetAmount,
                    x.Clinician is null
                        ? null
                        : $"{x.Clinician.FirstName} {x.Clinician.LastName}".Trim()
                ))
                .ToArray()
        );
    }
}
