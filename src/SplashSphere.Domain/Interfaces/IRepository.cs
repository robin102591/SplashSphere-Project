namespace SplashSphere.Domain.Interfaces;

/// <summary>
/// Generic repository interface for basic CRUD operations.
/// All reads use <c>AsNoTracking</c> — returned entities are detached from the
/// change tracker. Command handlers that need to modify an entity should call
/// <see cref="Update"/> after mutating the loaded entity so EF Core re-attaches it.
/// </summary>
/// <typeparam name="T">Entity type. Must be a reference type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Returns the entity with the given primary key, or <c>null</c> if not found.
    /// Uses <c>AsNoTracking</c> — call <see cref="Update"/> before saving mutations.
    /// </summary>
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the first entity matching <paramref name="predicate"/>, or <c>null</c>.
    /// Uses <c>AsNoTracking</c>.
    /// </summary>
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entities. Prefer filtered overloads in production to avoid large result sets.
    /// Uses <c>AsNoTracking</c>.
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists a new entity (added to the change tracker; saved on next SaveChanges).</summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks <paramref name="entity"/> as modified. If the entity was loaded with
    /// <c>AsNoTracking</c> (the default for reads in this repo), call this after
    /// mutating properties so EF Core re-attaches and generates an UPDATE.
    /// </summary>
    void Update(T entity);

    /// <summary>Marks <paramref name="entity"/> for deletion.</summary>
    void Remove(T entity);

    /// <summary>Returns <c>true</c> if any entity matches <paramref name="predicate"/>.</summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>Returns the count of entities matching <paramref name="predicate"/> (or all if null).</summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
}
