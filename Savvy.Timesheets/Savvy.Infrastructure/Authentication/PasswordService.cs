using Microsoft.AspNetCore.Identity;
using Savvy.Application.Authentication;
using Savvy.Domain;

namespace Savvy.Infrastructure.Authentication;

public sealed class PasswordService(IPasswordHasher<User> passwordHasher) : IPasswordService
{
    /// <summary>Verifies a supplied password against the user's stored Identity password hash.</summary>
    public bool Verify(User user, string password)
    {
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result
            is PasswordVerificationResult.Success
                or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
