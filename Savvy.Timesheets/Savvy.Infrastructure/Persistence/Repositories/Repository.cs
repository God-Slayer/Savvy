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

    /// <summary>Loads one entity by identifier without tracking it for updates.</summary>
    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    /// <summary>Loads all non-tracked entities from the repository set.</summary>
    public async Task<IReadOnlyList<TEntity>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await Entities.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>Loads the first non-tracked entity matching a predicate.</summary>
    public Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return Entities.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>Stages one new entity for insertion.</summary>
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Entities.AddAsync(entity, cancellationToken);
    }

    /// <summary>Stages multiple new entities for insertion.</summary>
    public Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        return Entities.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>Marks an entity for persistence of its changed values.</summary>
    public void Update(TEntity entity)
    {
        Entities.Update(entity);
    }

    /// <summary>Marks an entity as deleted without physically removing its row.</summary>
    public void SoftDelete(TEntity entity)
    {
        entity.IsDeleted = true;
        Entities.Update(entity);
    }
}
