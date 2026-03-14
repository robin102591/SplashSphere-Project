namespace SplashSphere.Domain.Interfaces;

/// <summary>
/// Repository for <see cref="ServicePricing"/> matrix rows.
/// Supports the pricing matrix lookup in <c>CreateTransactionCommandHandler</c>
/// (Step 2) and the bulk-replace endpoint <c>PUT /services/{id}/pricing</c>.
/// </summary>
public interface IServicePricingRepository : ITenantAwareRepository<ServicePricing>
{
    /// <summary>
    /// Returns the pricing row for the exact (service, vehicleType, size) combination,
    /// or <c>null</c> when no matrix entry exists (caller falls back to
    /// <c>Service.BasePrice</c>).
    /// Uses <c>AsNoTracking</c>.
    /// </summary>
    Task<ServicePricing?> GetByMatrixKeyAsync(
        string serviceId,
        string vehicleTypeId,
        string sizeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all pricing rows for a service across all vehicle type / size combinations.
    /// Used by the back-office pricing matrix editor to populate the grid.
    /// Uses <c>AsNoTracking</c>.
    /// </summary>
    Task<IReadOnlyList<ServicePricing>> GetAllForServiceAsync(
        string serviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the entire pricing matrix for <paramref name="serviceId"/> with
    /// <paramref name="rows"/>. Deletes all existing rows for the service first, then
    /// inserts the new set in a single operation. The caller is responsible for wrapping
    /// this in a transaction via <see cref="IUnitOfWork"/>.
    /// </summary>
    Task BulkUpsertAsync(
        string serviceId,
        IEnumerable<ServicePricing> rows,
        CancellationToken cancellationToken = default);
}
