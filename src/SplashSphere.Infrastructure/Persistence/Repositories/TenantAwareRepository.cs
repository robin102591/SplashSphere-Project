using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Domain.Interfaces;
using SplashSphere.SharedKernel.Results;
using SplashSphere.Infrastructure.Persistence;

namespace SplashSphere.Infrastructure.Persistence.Repositories;

/// <summary>
/// Base repository implementation backed by <see cref="ApplicationDbContext"/>.
/// All read paths call <c>AsNoTracking()</c> so EF Core does not pay the overhead
/// of identity-map bookkeeping for queries that only project or read data.
/// Command handlers that need to mutate an entity should call <see cref="Update"/>
/// after making property changes; EF Core will re-attach the entity in Modified state
/// and generate a full-row UPDATE on the next <c>SaveChangesAsync</c> call.
/// Tenant isolation is applied automatically through the global query filters
/// configured in <see cref="ApplicationDbContext.OnModelCreating"/>.
/// </summary>
/// <typeparam name="T">Entity type (must be a reference type).</typeparam>
public class TenantAwareRepository<T>(ApplicationDbContext context)
    : ITenantAwareRepository<T>
    where T : class
{
    protected readonly ApplicationDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    // ── Reads (all AsNoTracking) ──────────────────────────────────────────────

    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id, cancellationToken);

    public async Task<T?> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();

        if (filter is not null)
            query = query.Where(filter);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(items, totalCount, page, pageSize);
    }

    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .AnyAsync(predicate, cancellationToken);

    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default) =>
        predicate is null
            ? await DbSet.AsNoTracking().CountAsync(cancellationToken)
            : await DbSet.AsNoTracking().CountAsync(predicate, cancellationToken);

    // ── Writes (tracked) ──────────────────────────────────────────────────────

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await DbSet.AddAsync(entity, cancellationToken);

    /// <summary>
    /// Re-attaches <paramref name="entity"/> and marks all scalar properties modified.
    /// Generates a full-row UPDATE — suitable for the CQRS pattern where entities are
    /// loaded with <c>AsNoTracking</c>, mutated in the handler, then re-attached here.
    /// </summary>
    public void Update(T entity) =>
        Context.Update(entity);

    public void Remove(T entity) =>
        DbSet.Remove(entity);
}
