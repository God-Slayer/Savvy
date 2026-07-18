using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Savvy.Application.Common;
using Savvy.Application.PaymentRuns;
using Savvy.Application.Shifts;
using Savvy.Domain;

namespace Savvy.Api.Controllers;

[ApiController]
/// <summary>Exposes payment batch creation, retrieval, and processing operations.</summary>
public sealed class PaymentRunController(IPaymentRunService service) : ControllerBase
{
    [HttpPost("api/practices/{practiceId:guid}/payment-runs")]
    /// <summary>Creates a payment run for approved timesheets in a practice and period.</summary>
    public async Task<IActionResult> Create(
        Guid practiceId,
        PaymentRunCreateDto dto,
        CancellationToken ct
    )
    {
        var result = await service.CreateAsync(practiceId, dto, Context(), ct);
        return Respond(result, 201);
    }

    [HttpGet("api/payment-runs/{id:guid}")]
    /// <summary>Returns a payment run with its line items.</summary>
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await service.GetAsync(id, Context(), ct);
        return Respond(result);
    }

    [HttpPost("api/payment-runs/{id:guid}/process")]
    /// <summary>Marks a payment run as processed.</summary>
    public async Task<IActionResult> Process(Guid id, CancellationToken ct)
    {
        var result = await service.ProcessAsync(id, Context(), ct);
        return Respond(result);
    }

    /// <summary>Builds the application caller context from JWT claims.</summary>
    CallerContext Context()
    {
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id);
        Guid? p = Guid.TryParse(User.FindFirstValue("practice_id"), out var x) ? x : null;
        Enum.TryParse(User.FindFirstValue(ClaimTypes.Role), out UserRole role);
        return new(id, role, p);
    }

    /// <summary>Translates an application result into an HTTP response.</summary>
    IActionResult Respond<T>(Result<T> result, int success = 200)
    {
        if (result.IsSuccess)
            return StatusCode(success, result.Value);

        return result.ErrorCode switch
        {
            "not_found" => NotFound(new { error = result.ErrorMessage }),
            "forbidden" => Forbid(),
            "validation" => BadRequest(new { error = result.ErrorMessage }),
            "conflict" => Conflict(new { error = result.ErrorMessage }),
            _ => Problem(statusCode: 500),
        };
    }
}
