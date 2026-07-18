using Microsoft.EntityFrameworkCore;
using Savvy.Application.Persistence;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(TimesheetsDbContext dbContext)
    : Repository<User>(dbContext),
        IUserRepository
{
    public Task<User?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default
    ) =>
        Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => EF.Property<string>(user, "NormalizedEmail") == normalizedEmail,
                cancellationToken
            );
}
