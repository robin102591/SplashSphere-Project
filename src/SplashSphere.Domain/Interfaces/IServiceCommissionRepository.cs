namespace SplashSphere.Domain.Interfaces;

/// <summary>
/// Repository for <see cref="ServiceCommission"/> matrix rows.
/// Supports commission calculation in <c>CreateTransactionCommandHandler</c>
/// (Step 3) and the bulk-replace endpoint <c>PUT /services/{id}/commissions</c>.
/// No matrix entry for a combination means ₱0 commission for that cell.
/// </summary>
public interface IServiceCommissionRepository : ITenantAwareRepository<ServiceCommission>
{
    /// <summary>
    /// Returns the commission rule for the exact (service, vehicleType, size) combination,
    /// or <c>null</c> when no matrix entry exists (commission = ₱0 for that combination).
    /// Uses <c>AsNoTracking</c>.
    /// </summary>
    Task<ServiceCommission?> GetByMatrixKeyAsync(
        string serviceId,
        string vehicleTypeId,
        string sizeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all commission rows for a service across all vehicle type / size combinations.
    /// Used by the back-office commission matrix editor to populate the grid.
    /// Uses <c>AsNoTracking</c>.
    /// </summary>
    Task<IReadOnlyList<ServiceCommission>> GetAllForServiceAsync(
        string serviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the entire commission matrix for <paramref name="serviceId"/> with
    /// <paramref name="rows"/>. Deletes all existing rows for the service first, then
    /// inserts the new set. The caller is responsible for wrapping this in a transaction
    /// via <see cref="IUnitOfWork"/>.
    /// </summary>
    Task BulkUpsertAsync(
        string serviceId,
        IEnumerable<ServiceCommission> rows,
        CancellationToken cancellationToken = default);
}
