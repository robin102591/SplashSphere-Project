using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetServiceCostBreakdown;

public sealed class GetServiceCostBreakdownQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetServiceCostBreakdownQuery, ServiceCostBreakdownDto?>
{
    public async Task<ServiceCostBreakdownDto?> Handle(
        GetServiceCostBreakdownQuery request, CancellationToken cancellationToken)
    {
        var service = await db.Services
            .AsNoTracking()
            .Where(s => s.Id == request.ServiceId)
            .Select(s => new { s.Name, s.BasePrice })
            .FirstOrDefaultAsync(cancellationToken);

        if (service is null)
            return null;

        var sizes = await db.Sizes
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(cancellationToken);

        var pricings = await db.ServicePricings
            .AsNoTracking()
            .Where(p => p.ServiceId == request.ServiceId)
            .ToListAsync(cancellationToken);

        var usages = await db.ServiceSupplyUsages
            .AsNoTracking()
            .Where(u => u.ServiceId == request.ServiceId)
            .Include(u => u.SupplyItem)
            .ToListAsync(cancellationToken);

        var sizeCosts = new List<SizeCostDto>();

        foreach (var size in sizes)
        {
            // Find price for this size (any vehicle type — use first match or fallback to base price)
            var pricing = pricings.FirstOrDefault(p => p.SizeId == size.Id);
            var servicePrice = pricing?.Price ?? service.BasePrice;

            // Find usages for this size (fallback to null-size defaults)
            var sizeUsages = usages
                .Where(u => u.SizeId == size.Id || u.SizeId == null)
                .GroupBy(u => u.SupplyItemId)
                .Select(g => g.FirstOrDefault(u => u.SizeId == size.Id) ?? g.First())
                .ToList();

            var costLines = sizeUsages.Select(u => new SupplyCostLineDto(
                u.SupplyItem.Name,
                u.SupplyItem.Unit,
                u.QuantityPerUse,
                u.SupplyItem.AverageUnitCost,
                Math.Round(u.QuantityPerUse * u.SupplyItem.AverageUnitCost, 2, MidpointRounding.AwayFromZero)))
                .ToList();

            var supplyCost = costLines.Sum(c => c.LineCost);
            var grossMargin = servicePrice - supplyCost;
            var marginPercent = servicePrice > 0
                ? Math.Round(grossMargin / servicePrice * 100, 2, MidpointRounding.AwayFromZero)
                : 0;

            sizeCosts.Add(new SizeCostDto(
                size.Id,
                size.Name,
                servicePrice,
                supplyCost,
                EstimatedCommission: 0, // Commission calculation requires full matrix — placeholder
                grossMargin,
                marginPercent,
                costLines));
        }

        return new ServiceCostBreakdownDto(service.Name, service.BasePrice, sizeCosts);
    }
}
