using Savvy.Application.Common;

namespace Savvy.Application.Authentication;

public interface IAuthenticationService
{
    Task<Result<LoginResponseDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default
    );
}
