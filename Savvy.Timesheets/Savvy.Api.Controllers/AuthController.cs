using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Savvy.Application.Authentication;

namespace Savvy.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthenticationService authenticationService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    /// <summary>Authenticates a user and returns a signed JWT access token.</summary>
    public async Task<IActionResult> Login(
        LoginRequestDto request,
        CancellationToken cancellationToken
    )
    {
        var result = await authenticationService.LoginAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication failed",
                detail: result.ErrorMessage
            );
        }

        return Ok(result.Value);
    }
}
