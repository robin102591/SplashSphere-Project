namespace SplashSphere.Domain.Interfaces;

/// <summary>
/// Extends <see cref="IRepository{T}"/> with paged querying for tenant-scoped entities.
/// Tenant isolation is enforced automatically by the EF Core global query filters configured
/// on <c>ApplicationDbContext</c> — repositories do not filter by TenantId manually.
/// </summary>
/// <typeparam name="T">Tenant-scoped entity type.</typeparam>
public interface ITenantAwareRepository<T> : IRepository<T> where T : class
{
    /// <summary>
    /// Returns a page of entities, optionally filtered by <paramref name="filter"/>.
    /// Results are ordered by the database default (insertion order / PK) unless the
    /// caller applies ordering inside <paramref name="filter"/>.
    /// Uses <c>AsNoTracking</c>.
    /// </summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Maximum items per page.</param>
    /// <param name="filter">Optional additional predicate applied after the global tenant filter.</param>
    Task<PagedResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        CancellationToken cancellationToken = default);
}
