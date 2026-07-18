using System.Linq.Expressions;
using Savvy.Application.Persistence;
using Savvy.Application.Shifts;
using Savvy.Application.Timesheets;
using Savvy.Domain;

namespace Savvy.Tests;

public sealed class WorkflowTests
{
    [Fact]
    public void New_timesheets_start_submitted_and_new_shifts_start_open()
    {
        Assert.Equal(TimesheetStatus.Submitted, new Timesheet().Status);
        Assert.Equal(ShiftStatus.Open, new Shift().Status);
    }

    [Fact]
    public void Payment_statuses_have_expected_order_for_locking()
    {
        Assert.True((int)TimesheetStatus.Paid > (int)TimesheetStatus.Approved);
    }

    [Fact]
    public async Task Submit_is_idempotent_for_existing_business_reference()
    {
        var shift = NewShift();
        var existing = new Timesheet
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            BusinessReference = "REF-1",
            ActualStartUtc = shift.ScheduledStartUtc,
            ActualEndUtc = shift.ScheduledEndUtc,
        };
        var repo = new TimesheetRepo { ExistingReference = existing };
        var service = new TimesheetService(repo, new ShiftRepo(shift), new UnitOfWork());
        var result = await service.SubmitAsync(
            shift.Id,
            new TimesheetSubmitDto(
                "REF-1",
                shift.ScheduledStartUtc,
                shift.ScheduledEndUtc,
                0,
                null
            ),
            new CallerContext(shift.ClinicianId, UserRole.Clinician, shift.PracticeId)
        );
        Assert.True(result.IsSuccess);
        Assert.Equal(existing.Id, result.Value!.Id);
        Assert.Empty(repo.Added);
    }

    [Fact]
    public async Task Clinician_cannot_submit_for_another_clinician()
    {
        var shift = NewShift();
        var service = new TimesheetService(
            new TimesheetRepo(),
            new ShiftRepo(shift),
            new UnitOfWork()
        );
        var result = await service.SubmitAsync(
            shift.Id,
            ValidDto(shift, "REF-2"),
            new CallerContext(Guid.NewGuid(), UserRole.Clinician, shift.PracticeId)
        );
        Assert.False(result.IsSuccess);
        Assert.Equal("forbidden", result.ErrorCode);
    }

    [Fact]
    public async Task Approval_sets_timesheet_approved_and_shift_completed()
    {
        var shift = NewShift();
        var sheet = new Timesheet
        {
            Id = Guid.NewGuid(),
            ShiftId = shift.Id,
            BusinessReference = "REF-3",
            ActualStartUtc = shift.ScheduledStartUtc,
            ActualEndUtc = shift.ScheduledEndUtc,
        };
        var service = new TimesheetService(
            new TimesheetRepo { ById = sheet },
            new ShiftRepo(shift),
            new UnitOfWork()
        );
        var result = await service.ApproveAsync(
            sheet.Id,
            new CallerContext(Guid.NewGuid(), UserRole.PracticeManager, shift.PracticeId)
        );
        Assert.True(result.IsSuccess);
        Assert.Equal(TimesheetStatus.Approved, sheet.Status);
        Assert.Equal(ShiftStatus.Completed, shift.Status);
    }

    private static Shift NewShift() =>
        new()
        {
            PracticeId = Guid.NewGuid(),
            ClinicianId = Guid.NewGuid(),
            ScheduledStartUtc = DateTime.UtcNow,
            ScheduledEndUtc = DateTime.UtcNow.AddHours(8),
            Role = "Nurse",
            Location = "Clinic",
            HourlyRate = 50,
        };

    private static TimesheetSubmitDto ValidDto(Shift s, string reference) =>
        new(reference, s.ScheduledStartUtc, s.ScheduledEndUtc, 30, "notes");

    private sealed class ShiftRepo(Shift shift) : IShiftRepository
    {
        public ShiftRepo()
            : this(NewShift()) { }

        public Task<Shift?> GetByIdAsync(Guid id, CancellationToken c = default) =>
            Task.FromResult<Shift?>(shift.Id == id ? shift : null);

        public Task<IReadOnlyList<Shift>> ListAsync(CancellationToken c = default) =>
            Task.FromResult<IReadOnlyList<Shift>>([shift]);

        public Task<Shift?> FirstOrDefaultAsync(
            Expression<Func<Shift, bool>> p,
            CancellationToken c = default
        ) => Task.FromResult<Shift?>(shift);

        public Task AddAsync(Shift e, CancellationToken c = default) => Task.CompletedTask;

        public Task AddRangeAsync(IEnumerable<Shift> e, CancellationToken c = default) =>
            Task.CompletedTask;

        public void Update(Shift e) { }

        public void SoftDelete(Shift e) => e.IsDeleted = true;

        public Task<IReadOnlyList<Shift>> ListByPracticeAsync(
            Guid p,
            CancellationToken c = default
        ) => ListAsync(c);

        public Task<IReadOnlyList<Shift>> ListByClinicianAsync(
            Guid id,
            CancellationToken c = default
        ) => ListAsync(c);

        public Task<Shift?> GetByIdWithDetailsAsync(Guid id, CancellationToken c = default) =>
            GetByIdAsync(id, c);
    }

    private sealed class TimesheetRepo : ITimesheetRepository
    {
        public Timesheet? ExistingReference { get; set; }
        public Timesheet? ById { get; set; }
        public List<Timesheet> Added { get; } = [];

        public Task<Timesheet?> GetByIdAsync(Guid id, CancellationToken c = default) =>
            Task.FromResult(ById?.Id == id ? ById : null);

        public Task<IReadOnlyList<Timesheet>> ListAsync(CancellationToken c = default) =>
            Task.FromResult<IReadOnlyList<Timesheet>>(Added);

        public Task<Timesheet?> FirstOrDefaultAsync(
            Expression<Func<Timesheet, bool>> p,
            CancellationToken c = default
        ) => Task.FromResult<Timesheet?>(null);

        public Task AddAsync(Timesheet e, CancellationToken c = default)
        {
            Added.Add(e);
            return Task.CompletedTask;
        }

        public Task AddRangeAsync(IEnumerable<Timesheet> e, CancellationToken c = default) =>
            Task.CompletedTask;

        public void Update(Timesheet e) { }

        public void SoftDelete(Timesheet e) => e.IsDeleted = true;

        public Task<Timesheet?> GetByBusinessReferenceAsync(
            string r,
            CancellationToken c = default
        ) => Task.FromResult(ExistingReference?.BusinessReference == r ? ExistingReference : null);

        public Task<Timesheet?> GetByShiftIdAsync(Guid id, CancellationToken c = default) =>
            Task.FromResult<Timesheet?>(null);

        public Task<IReadOnlyList<Timesheet>> GetByPracticeAsync(
            Guid practiceId,
            CancellationToken c = default
        ) => Task.FromResult<IReadOnlyList<Timesheet>>(Added);

        public Task<IReadOnlyList<Timesheet>> GetApprovedForPaymentAsync(
            Guid p,
            DateOnly s,
            DateOnly e,
            CancellationToken c = default
        ) => Task.FromResult<IReadOnlyList<Timesheet>>([]);
    }

    private sealed class UnitOfWork : IUnitOfWork
    {
        public IRepository<Practice> Practices => throw new NotImplementedException();
        public IUserRepository Users => throw new NotImplementedException();
        public IShiftRepository Shifts => throw new NotImplementedException();
        public ITimesheetRepository Timesheets => throw new NotImplementedException();
        public IPaymentRunRepository PaymentRuns => throw new NotImplementedException();

        public Task<int> SaveChangesAsync(CancellationToken c = default) => Task.FromResult(1);
    }
}
