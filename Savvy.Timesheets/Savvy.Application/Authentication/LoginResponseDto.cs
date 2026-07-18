namespace Savvy.Application.Authentication;

public sealed record LoginResponseDto(
    string AccessToken,
    DateTime ExpiresUtc,
    UserResponseDto User
);
