using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SplashSphere.SharedKernel.Abstractions;

namespace SplashSphere.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically stamps <see cref="IAuditableEntity.CreatedAt"/>
/// (on insert) and <see cref="IAuditableEntity.UpdatedAt"/> (on insert and update) with
/// the current UTC time before every save.
/// <para>
/// Registered as a singleton in DI and added to <c>ApplicationDbContext</c> via
/// <c>DbContextOptionsBuilder.AddInterceptors</c>. Both the synchronous
/// (<see cref="SavingChanges"/>) and asynchronous (<see cref="SavingChangesAsync"/>)
/// paths are covered so that seed data, migrations, and tests work regardless of which
/// overload they call.
/// </para>
/// </summary>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    // ── Synchronous path (seed data, migrations, tests) ───────────────────────

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        StampAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    // ── Asynchronous path (all production code paths) ─────────────────────────

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        StampAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // ── Implementation ────────────────────────────────────────────────────────

    private static void StampAuditFields(DbContext? context)
    {
        if (context is null)
            return;

        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;

            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
    }
}
