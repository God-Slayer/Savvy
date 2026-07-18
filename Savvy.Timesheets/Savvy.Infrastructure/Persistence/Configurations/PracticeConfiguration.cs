using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence.Configurations;

internal sealed class PracticeConfiguration : EntityConfigurationBase<Practice>
{
    public override void Configure(EntityTypeBuilder<Practice> builder)
    {
        base.Configure(builder);
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
    }
}
