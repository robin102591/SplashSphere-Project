namespace SplashSphere.Domain.Interfaces;

/// <summary>
/// Repository for <see cref="Transaction"/> with operations specific to the POS
/// transaction aggregate (full-graph eager-loading, transaction-number sequencing,
/// and daily branch summary queries).
/// </summary>
public interface ITransactionRepository : ITenantAwareRepository<Transaction>
{
    /// <summary>
    /// Loads the full transaction aggregate in a single query using eager loading:
    /// Branch, Cashier, Car (with VehicleType + Size), Customer, Services (with Service
    /// and per-employee assignments), Packages (with Package and assignments),
    /// Merchandise items, employee commission summaries, Payments, and linked QueueEntry.
    /// Uses <c>AsNoTracking</c> — intended for read (detail/receipt) queries.
    /// Returns <c>null</c> when not found (or filtered out by tenant).
    /// </summary>
    Task<Transaction?> GetWithDetailsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the next 1-based sequence number for a branch on a calendar date.
    /// Used by <c>CreateTransactionCommandHandler</c> to generate the human-readable
    /// <c>{BranchCode}-{YYYYMMDD}-{Sequence}</c> transaction number.
    /// <para>
    /// The count-based approach has a theoretical race condition under extreme concurrency;
    /// the unique index on <c>(TenantId, TransactionNumber)</c> serves as the conflict
    /// guard — handlers catch the unique violation and retry with a fresh sequence call.
    /// </para>
    /// </summary>
    /// <param name="branchId">Branch whose daily sequence counter to increment.</param>
    /// <param name="date">Calendar date (Asia/Manila) for which to count existing transactions.</param>
    Task<int> GetNextDailySequenceAsync(string branchId, DateOnly date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all transactions for a branch on the given calendar date, ordered by
    /// creation time. Used by the daily summary and cashier history endpoints.
    /// Uses <c>AsNoTracking</c>.
    /// </summary>
    Task<IReadOnlyList<Transaction>> GetByBranchAndDateAsync(
        string branchId,
        DateOnly date,
        CancellationToken cancellationToken = default);
}
