namespace Savvy.Application.Authentication;

public sealed record UserResponseDto(
    Guid Id,
    Guid? PracticeId,
    string Email,
    string FirstName,
    string LastName,
    string Role
);
