using System.ComponentModel.DataAnnotations;

namespace Savvy.Application.Authentication;

public sealed class LoginRequestDto
{
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(256, MinimumLength = 1)]
    public string Password { get; init; } = string.Empty;
}
