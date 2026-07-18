using Savvy.Application.Common;
using Savvy.Application.Persistence;

namespace Savvy.Application.Authentication;

public sealed class AuthenticationService(
    IUserRepository users,
    IPasswordService passwordService,
    IAccessTokenGenerator accessTokenGenerator
) : IAuthenticationService
{
    /// <summary>Validates credentials and issues an access token for the authenticated user.</summary>
    public async Task<Result<LoginResponseDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        var email = request.Email.Trim().ToUpperInvariant();
        var user = await users.GetByNormalizedEmailAsync(email, cancellationToken);

        if (user is null || !passwordService.Verify(user, request.Password))
        {
            return Result<LoginResponseDto>.Failure(
                "invalid_credentials",
                "Email or password is incorrect."
            );
        }

        var token = accessTokenGenerator.Generate(user);
        var userResponse = new UserResponseDto(
            user.Id,
            user.PracticeId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString()
        );

        return Result<LoginResponseDto>.Success(
            new LoginResponseDto(token.Value, token.ExpiresUtc, userResponse)
        );
    }
}
