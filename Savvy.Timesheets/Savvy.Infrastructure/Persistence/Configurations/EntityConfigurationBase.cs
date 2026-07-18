using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Configurations;

internal abstract class EntityConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();
        builder.Property(entity => entity.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.HasQueryFilter(entity => !entity.IsDeleted);
    }
}
