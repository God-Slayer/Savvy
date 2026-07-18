using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Savvy.Application.Persistence;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Repositories;

public class Repository<TEntity>(TimesheetsDbContext dbContext) : IRepository<TEntity>
    where TEntity : BaseEntity
{
    protected TimesheetsDbContext DbContext { get; } = dbContext;
    protected DbSet<TEntity> Entities { get; } = dbContext.Set<TEntity>();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await Entities.AsNoTracking().ToListAsync(cancellationToken);
    }

    public Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return Entities.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Entities.AddAsync(entity, cancellationToken);
    }

    public Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        return Entities.AddRangeAsync(entities, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        Entities.Update(entity);
    }

    public void SoftDelete(TEntity entity)
    {
        entity.IsDeleted = true;
        Entities.Update(entity);
    }
}
