using Microsoft.AspNetCore.Identity;
using Savvy.Application.Authentication;
using Savvy.Domain;

namespace Savvy.Infrastructure.Authentication;

public sealed class PasswordService(IPasswordHasher<User> passwordHasher) : IPasswordService
{
    // Verifies a plaintext password against the stored password hash.
    public bool Verify(User user, string password)
    {
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result
            is PasswordVerificationResult.Success
                or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
