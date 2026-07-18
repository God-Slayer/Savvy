using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Savvy.Application.Shifts;
using Savvy.Domain;

namespace Savvy.Api.Controllers;

[ApiController]
/// <summary>Exposes shift scheduling and retrieval operations over HTTP.</summary>
public sealed class ShiftController(IShiftService service) : ControllerBase
{
    [HttpGet("api/practices/{practiceId:guid}/shifts")]
    /// <summary>Lists shifts visible to the authenticated caller for a practice.</summary>
    public async Task<IActionResult> List(
        Guid practiceId,
        [FromQuery] string? status,
        CancellationToken ct
    )
    {
        var r = await service.ListAsync(practiceId, Context(), status, ct);
        return Respond(r);
    }

    [HttpGet("api/shifts/{shiftId:guid}")]
    /// <summary>Returns a single shift visible to the authenticated caller.</summary>
    public async Task<IActionResult> Get(Guid shiftId, CancellationToken ct)
    {
        var r = await service.GetAsync(shiftId, Context(), ct);
        return Respond(r);
    }

    [HttpPost("api/practices/{practiceId:guid}/shifts")]
    /// <summary>Creates an open shift for a practice.</summary>
    public async Task<IActionResult> Create(Guid practiceId, ShiftCreateDto d, CancellationToken ct)
    {
        var r = await service.CreateAsync(practiceId, d, Context(), ct);
        return Respond(r, 201);
    }

    [HttpPut("api/shifts/{shiftId:guid}")]
    /// <summary>Updates an editable shift using optimistic concurrency.</summary>
    public async Task<IActionResult> Update(Guid shiftId, ShiftUpdateDto d, CancellationToken ct)
    {
        var r = await service.UpdateAsync(shiftId, d, Context(), ct);
        return Respond(r);
    }

    [HttpDelete("api/shifts/{shiftId:guid}")]
    /// <summary>Soft-deletes an editable shift.</summary>
    public async Task<IActionResult> Delete(Guid shiftId, CancellationToken ct)
    {
        var r = await service.DeleteAsync(shiftId, Context(), ct);
        return Respond(r);
    }

    /// <summary>Builds the application caller context from JWT claims.</summary>
    CallerContext Context()
    {
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id);
        Guid? p = Guid.TryParse(User.FindFirstValue("practice_id"), out var pid) ? pid : null;
        Enum.TryParse(User.FindFirstValue(ClaimTypes.Role), out UserRole role);
        return new(id, role, p);
    }

    /// <summary>Translates an application result into an HTTP response.</summary>
    IActionResult Respond<T>(Application.Common.Result<T> r, int success = 200)
    {
        if (r.IsSuccess)
            return StatusCode(success, r.Value);
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
