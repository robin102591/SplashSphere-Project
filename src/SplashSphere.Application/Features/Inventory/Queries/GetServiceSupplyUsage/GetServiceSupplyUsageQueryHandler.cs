using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetServiceSupplyUsage;

public sealed class GetServiceSupplyUsageQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetServiceSupplyUsageQuery, IReadOnlyList<ServiceSupplyUsageDto>>
{
    public async Task<IReadOnlyList<ServiceSupplyUsageDto>> Handle(
        GetServiceSupplyUsageQuery request, CancellationToken cancellationToken)
    {
        var usages = await db.ServiceSupplyUsages
            .AsNoTracking()
            .Where(u => u.ServiceId == request.ServiceId)
            .Select(u => new
            {
                u.SupplyItemId,
                SupplyItemName = u.SupplyItem.Name,
                Unit = u.SupplyItem.Unit,
                u.SizeId,
                SizeName = u.Size != null ? u.Size.Name : null,
                u.QuantityPerUse,
            })
            .ToListAsync(cancellationToken);

        var grouped = usages
            .GroupBy(u => new { u.SupplyItemId, u.SupplyItemName, u.Unit })
            .Select(g => new ServiceSupplyUsageDto(
                g.Key.SupplyItemId,
                g.Key.SupplyItemName,
                g.Key.Unit,
                g.Select(u => new SizeUsageDto(u.SizeId, u.SizeName, u.QuantityPerUse))
                    .ToList()))
            .ToList();

        return grouped;
    }
}
