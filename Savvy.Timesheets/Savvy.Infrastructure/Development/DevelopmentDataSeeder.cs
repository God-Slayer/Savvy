using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Savvy.Domain;
using Savvy.Infrastructure.Persistence;

namespace Savvy.Infrastructure.Development;

/// <summary>Creates the stable, self-contained demo dataset used by local development.</summary>
public sealed class DevelopmentDataSeeder(
    TimesheetsDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IConfiguration configuration
)
{
    private static readonly Guid PracticeId = Guid.Parse("8950DA2B-112B-4F24-B48D-B85451E9FE62");
    private static readonly Guid AdminId = Guid.Parse("03A6E7CC-D358-4ACB-AE2E-D23FBDF67A1E");
    private static readonly Guid ManagerId = Guid.Parse("FAE49192-C547-49A7-8703-D487F0502919");
    private static readonly Guid ClinicianId = Guid.Parse("1B1F87E4-B716-4B44-9CC0-D761F635275B");
    private static readonly Guid ShiftOneId = Guid.Parse("AF37BA8E-72F9-4B80-AE65-DD15ED95AB13");
    private static readonly Guid ShiftTwoId = Guid.Parse("80FD2584-0AAF-4658-971D-2801A9BE7022");
    private static readonly Guid TimesheetOneId = Guid.Parse(
        "3E7E9BB8-E81F-40DE-B02F-028FFA8230BA"
    );
    private static readonly Guid TimesheetTwoId = Guid.Parse(
        "65612B57-3A67-4B3D-8B08-FA011FC161BA"
    );
    private static readonly Guid PaymentRunId = Guid.Parse("D1E5D67A-4811-48F7-9C5C-C55742E692EF");
    private static readonly Guid PaymentLineId = Guid.Parse("EB238144-8670-4765-952C-530984FEA70E");

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var password = configuration["DevelopmentSeed:DemoPassword"];
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("DevelopmentSeed:DemoPassword is not configured.");

        var practice = await UpsertPractice(cancellationToken);
        await UpsertUser(
            AdminId,
            "admin@savvy.local",
            "Savvy",
            "Admin",
            UserRole.Admin,
            null,
            password,
            cancellationToken
        );
        await UpsertUser(
            ManagerId,
            "manager@savvy.local",
            "Morgan",
            "Manager",
            UserRole.PracticeManager,
            practice.Id,
            password,
            cancellationToken
        );
        await UpsertUser(
            ClinicianId,
            "clinician@savvy.local",
            "Casey",
            "Clinician",
            UserRole.Clinician,
            practice.Id,
            password,
            cancellationToken
        );

        await UpsertShift(
            ShiftOneId,
            practice.Id,
            ClinicianId,
            new DateTime(2026, 7, 17, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 17, 16, 0, 0, DateTimeKind.Utc),
            ShiftStatus.Completed,
            cancellationToken
        );
        await UpsertShift(
            ShiftTwoId,
            practice.Id,
            ClinicianId,
            new DateTime(2026, 7, 18, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 18, 17, 0, 0, DateTimeKind.Utc),
            ShiftStatus.Completed,
            cancellationToken
        );

        await UpsertTimesheet(
            TimesheetOneId,
            ShiftOneId,
            "DEMO-TS-000",
            new DateTime(2026, 7, 17, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 17, 16, 0, 0, DateTimeKind.Utc),
            30,
            "DEMO-TS-000",
            TimesheetStatus.Approved,
            cancellationToken
        );
        await UpsertTimesheet(
            TimesheetTwoId,
            ShiftTwoId,
            "DEMO-TS-001",
            new DateTime(2026, 7, 18, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 18, 17, 0, 0, DateTimeKind.Utc),
            0,
            "DEMO-TS-001",
            TimesheetStatus.Paid,
            cancellationToken
        );

        var run = await dbContext.PaymentRuns.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == PaymentRunId, cancellationToken);
        run ??= new PaymentRun { Id = PaymentRunId };
        run.PracticeId = practice.Id;
        run.BusinessReference = "DEMO-PR-001";
        run.RequestHash = "DEMO-PR-001";
        run.PeriodStartDate = new DateOnly(2026, 7, 18);
        run.PeriodEndDate = new DateOnly(2026, 7, 18);
        run.PercentageFeeRate = 0m;
        run.FixedFeeAmount = 0m;
        run.TotalGrossAmount = 360m;
        run.TotalFeeAmount = 0m;
        run.TotalNetAmount = 360m;
        run.Currency = "GBP";
        run.Status = PaymentRunStatus.Processed;
        if (dbContext.Entry(run).State == EntityState.Detached)
            dbContext.PaymentRuns.Add(run);
        var line =
            await dbContext
                .PaymentRunLines.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == PaymentLineId, cancellationToken)
            ?? new PaymentRunLine { Id = PaymentLineId };
        line.PaymentRunId = PaymentRunId;
        line.TimesheetId = TimesheetTwoId;
        line.ShiftId = ShiftTwoId;
        line.ClinicianId = ClinicianId;
        line.HoursWorked = 8m;
        line.HourlyRate = 45m;
        line.GrossAmount = 360m;
        line.PercentageFeeAmount = 0m;
        line.FixedFeeAmount = 0m;
        line.TotalFeeAmount = 0m;
        line.NetAmount = 360m;
        if (dbContext.Entry(line).State == EntityState.Detached)
            dbContext.PaymentRunLines.Add(line);

        // A timesheet already represented by a payment-run line has been paid.
        var paidTimesheet = await dbContext.Timesheets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == line.TimesheetId, cancellationToken);
        if (paidTimesheet is not null)
            paidTimesheet.Status = TimesheetStatus.Paid;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Practice> UpsertPractice(CancellationToken ct)
    {
        var p =
            await dbContext
                .Practices.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == PracticeId, ct)
            ?? new Practice { Id = PracticeId };
        p.Name = "Savvy Dental";
        if (dbContext.Entry(p).State == EntityState.Detached)
            dbContext.Practices.Add(p);
        await dbContext.SaveChangesAsync(ct);
        return p;
    }

    private async Task UpsertUser(
        Guid id,
        string email,
        string first,
        string last,
        UserRole role,
        Guid? practiceId,
        string password,
        CancellationToken ct
    )
    {
        var u =
            await dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? new User { Id = id };
        u.Email = email;
        u.FirstName = first;
        u.LastName = last;
        u.Role = role;
        u.PracticeId = practiceId;
        if (string.IsNullOrWhiteSpace(u.PasswordHash))
            u.PasswordHash = passwordHasher.HashPassword(u, password);
        if (dbContext.Entry(u).State == EntityState.Detached)
            dbContext.Users.Add(u);
        await dbContext.SaveChangesAsync(ct);
    }

    private async Task UpsertShift(
        Guid id,
        Guid practiceId,
        Guid clinicianId,
        DateTime start,
        DateTime end,
        ShiftStatus status,
        CancellationToken ct
    )
    {
        var s =
            await dbContext.Shifts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? new Shift { Id = id };
        s.PracticeId = practiceId;
        s.ClinicianId = clinicianId;
        s.ScheduledStartUtc = start;
        s.ScheduledEndUtc = end;
        s.Role = "Dental Clinician";
        s.Location = "Savvy Dental - Main";
        s.HourlyRate = 45m;
        s.Status = status;
        if (dbContext.Entry(s).State == EntityState.Detached)
            dbContext.Shifts.Add(s);
        await dbContext.SaveChangesAsync(ct);
    }

    private async Task UpsertTimesheet(
        Guid id,
        Guid shiftId,
        string reference,
        DateTime start,
        DateTime end,
        int breakMinutes,
        string notes,
        TimesheetStatus status,
        CancellationToken ct
    )
    {
        var t =
            await dbContext.Timesheets.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? new Timesheet { Id = id };
        t.ShiftId = shiftId;
        t.BusinessReference = reference;
        t.RequestHash = reference;
        t.ActualStartUtc = start;
        t.ActualEndUtc = end;
        t.UnpaidBreakMinutes = breakMinutes;
        t.Notes = notes;
        t.Status = status;
        if (dbContext.Entry(t).State == EntityState.Detached)
            dbContext.Timesheets.Add(t);
        await dbContext.SaveChangesAsync(ct);
    }
}
