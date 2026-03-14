using Microsoft.EntityFrameworkCore;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Interfaces;

namespace SplashSphere.Infrastructure.Persistence.Repositories;

/// <summary>
/// Concrete repository for <see cref="ServiceCommission"/> matrix rows.
/// <see cref="GetByMatrixKeyAsync"/> is called once per service line item during
/// transaction creation (Step 3 of the algorithm). A <c>null</c> result means
/// ₱0 commission for that (service, vehicleType, size) combination.
/// </summary>
public sealed class ServiceCommissionRepository(ApplicationDbContext context)
    : TenantAwareRepository<ServiceCommission>(context), IServiceCommissionRepository
{
    public async Task<ServiceCommission?> GetByMatrixKeyAsync(
        string serviceId,
        string vehicleTypeId,
        string sizeId,
        CancellationToken cancellationToken = default) =>
        await Context.ServiceCommissions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                sc => sc.ServiceId == serviceId
                   && sc.VehicleTypeId == vehicleTypeId
                   && sc.SizeId == sizeId,
                cancellationToken);

    public async Task<IReadOnlyList<ServiceCommission>> GetAllForServiceAsync(
        string serviceId,
        CancellationToken cancellationToken = default) =>
        await Context.ServiceCommissions
            .AsNoTracking()
            .Where(sc => sc.ServiceId == serviceId)
            .ToListAsync(cancellationToken);

    public async Task BulkUpsertAsync(
        string serviceId,
        IEnumerable<ServiceCommission> rows,
        CancellationToken cancellationToken = default)
    {
        // Delete-then-insert: replace the entire commission matrix for this service.
        // EF Core 7+ applies the global tenant query filter to ExecuteDeleteAsync.
        await Context.ServiceCommissions
            .Where(sc => sc.ServiceId == serviceId)
            .ExecuteDeleteAsync(cancellationToken);

        await Context.ServiceCommissions.AddRangeAsync(rows, cancellationToken);
    }
}
