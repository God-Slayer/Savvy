using Microsoft.EntityFrameworkCore;
using Savvy.Domain;

namespace Savvy.Infrastructure.Persistence;

/// <summary>Entity Framework database context for practices, users, shifts, timesheets, and payments.</summary>
public sealed class TimesheetsDbContext(DbContextOptions<TimesheetsDbContext> options)
    : DbContext(options)
{
    public DbSet<Practice> Practices => Set<Practice>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<PaymentRun> PaymentRuns => Set<PaymentRun>();
    public DbSet<PaymentRunLine> PaymentRunLines => Set<PaymentRunLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TimesheetsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyEntityValues();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    )
    {
        ApplyEntityValues();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyEntityValues()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.UpdatedAtUtc = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.CreatedAtUtc).IsModified = false;
                entry.Entity.UpdatedAtUtc = now;
            }

            if (
                entry.Entity is User user
                && entry.State is EntityState.Added or EntityState.Modified
            )
            {
                entry.Property("NormalizedEmail").CurrentValue = user
                    .Email.Trim()
                    .ToUpperInvariant();
            }
        }
    }
}
