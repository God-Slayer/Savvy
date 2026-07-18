using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Savvy.Application.Common;
using Savvy.Application.Shifts;
using Savvy.Application.Timesheets;
using Savvy.Domain;

namespace Savvy.Api.Controllers;

[ApiController]
public sealed class TimesheetController(ITimesheetService service) : ControllerBase
{
    [HttpPost("api/shifts/{shiftId:guid}/timesheet")]
    /// <summary>Submits a clinician timesheet for an assigned shift.</summary>
    public async Task<IActionResult> Submit(
        Guid shiftId,
        TimesheetSubmitDto dto,
        CancellationToken ct
    )
    {
        var r = await service.SubmitAsync(shiftId, dto, Ctx(), ct);
        return Respond(r, 201);
    }

    [HttpGet("api/timesheets/{id:guid}")]
    /// <summary>Returns a timesheet visible to the authenticated caller.</summary>
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        return Respond(await service.GetAsync(id, Ctx(), ct));
    }

    [HttpPost("api/timesheets/{id:guid}/approve")]
    /// <summary>Approves a submitted timesheet for a manager or administrator.</summary>
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        return Respond(await service.ApproveAsync(id, Ctx(), ct));
    }

    // Builds the caller scope from the authenticated user's claims.
    private CallerContext Ctx()
    {
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id);
        Guid? p = Guid.TryParse(User.FindFirstValue("practice_id"), out var pid) ? pid : null;
        Enum.TryParse(User.FindFirstValue(ClaimTypes.Role), out UserRole role);
        return new(id, role, p);
    }

    // Converts an application result into the appropriate HTTP response.
    private IActionResult Respond<T>(Result<T> r, int success = 200)
    {
        if (r.IsSuccess)
        {
            return StatusCode(success, r.Value);
        }

        return r.ErrorCode switch
        {
            "not_found" => NotFound(new { error = r.ErrorMessage }),
            "forbidden" => Forbid(),
            "validation" => BadRequest(new { error = r.ErrorMessage }),
            "conflict" => Conflict(new { error = r.ErrorMessage }),
            _ => Problem(statusCode: 500),
        };
    }
}
