namespace SplashSphere.Domain.Interfaces;

/// <summary>
/// Abstracts the database session boundary so command handlers can flush all
/// pending changes atomically and, when needed, wrap multiple operations in an
/// explicit database transaction.
/// <para>
/// Typical command handler usage (no explicit transaction):
/// <code>
/// repository.Update(entity);
/// await unitOfWork.SaveChangesAsync(ct);
/// </code>
/// Typical multi-step usage (with explicit transaction):
/// <code>
/// await unitOfWork.BeginTransactionAsync(ct);
/// try
/// {
///     // ... perform multiple repo operations ...
///     await unitOfWork.SaveChangesAsync(ct);
///     await unitOfWork.CommitTransactionAsync(ct);
/// }
/// catch
/// {
///     await unitOfWork.RollbackTransactionAsync(ct);
///     throw;
/// }
/// </code>
/// </para>
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Flushes all tracked changes to the database.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Starts an explicit database transaction. Required when a single logical
    /// operation spans multiple <see cref="SaveChangesAsync"/> calls.</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Commits the current transaction. Throws if no transaction is active.</summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Rolls back the current transaction. Safe to call even when no transaction is active.</summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
