using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Savvy.Application.Reporting;
using Savvy.Domain;

namespace Savvy.Api.Controllers;

[ApiController]
public sealed class ReportingController(IReportingService service) : ControllerBase
{
    [HttpGet("api/practices/{practiceId:guid}/reports/summary")]
    /// <summary>Returns practice-scoped timesheet and payment summaries for a UTC date range.</summary>
    public async Task<IActionResult> Summary(
        Guid practiceId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken ct
    )
    {
        var role = Enum.TryParse<UserRole>(User.FindFirstValue(ClaimTypes.Role), out var r)
            ? r
            : UserRole.Clinician;
        var cp = Guid.TryParse(User.FindFirstValue("practice_id"), out var p) ? p : (Guid?)null;
        if (role == UserRole.Clinician || (role == UserRole.PracticeManager && cp != practiceId))
            return Forbid();
        var f = from ?? new DateOnly(2000, 1, 1);
        var t = to ?? new DateOnly(2100, 1, 1);
        if (t < f)
            return BadRequest(new { error = "Invalid date range." });
        return Ok(await service.GetSummaryAsync(practiceId, f, t, ct));
    }
}
