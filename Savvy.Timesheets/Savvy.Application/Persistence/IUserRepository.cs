using Savvy.Domain;

namespace Savvy.Application.Persistence;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
}
