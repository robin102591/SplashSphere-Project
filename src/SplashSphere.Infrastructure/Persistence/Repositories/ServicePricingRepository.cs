using Microsoft.EntityFrameworkCore;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Interfaces;

namespace SplashSphere.Infrastructure.Persistence.Repositories;

/// <summary>
/// Concrete repository for <see cref="ServicePricing"/> matrix rows.
/// The matrix lookup (<see cref="GetByMatrixKeyAsync"/>) is the hot path called
/// once per service line item during transaction creation (Step 2 of the algorithm).
/// The composite index on (ServiceId, VehicleTypeId, SizeId) makes each call O(1).
/// </summary>
public sealed class ServicePricingRepository(ApplicationDbContext context)
    : TenantAwareRepository<ServicePricing>(context), IServicePricingRepository
{
    public async Task<ServicePricing?> GetByMatrixKeyAsync(
        string serviceId,
        string vehicleTypeId,
        string sizeId,
        CancellationToken cancellationToken = default) =>
        await Context.ServicePricings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                sp => sp.ServiceId == serviceId
                   && sp.VehicleTypeId == vehicleTypeId
                   && sp.SizeId == sizeId,
                cancellationToken);

    public async Task<IReadOnlyList<ServicePricing>> GetAllForServiceAsync(
        string serviceId,
        CancellationToken cancellationToken = default) =>
        await Context.ServicePricings
            .AsNoTracking()
            .Where(sp => sp.ServiceId == serviceId)
            .ToListAsync(cancellationToken);

    public async Task BulkUpsertAsync(
        string serviceId,
        IEnumerable<ServicePricing> rows,
        CancellationToken cancellationToken = default)
    {
        // Delete-then-insert: replace the entire matrix for this service atomically.
        // EF Core 7+ applies the global tenant query filter to ExecuteDeleteAsync,
        // so only the current tenant's rows are removed.
        await Context.ServicePricings
            .Where(sp => sp.ServiceId == serviceId)
            .ExecuteDeleteAsync(cancellationToken);

        await Context.ServicePricings.AddRangeAsync(rows, cancellationToken);
    }
}
