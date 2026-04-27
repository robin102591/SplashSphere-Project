using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Services.Queries.GetServicesWithPricing;

public sealed class GetServicesWithPricingQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<GetServicesWithPricingQuery, ConnectServicesWithPricingDto?>
{
    private const string PriceModeExact = "exact";
    private const string PriceModeEstimate = "estimate";

    public async Task<ConnectServicesWithPricingDto?> Handle(
        GetServicesWithPricingQuery request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated) return null;

        var userId = connectUser.ConnectUserId;

        var vehicle = await db.ConnectVehicles
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                v => v.Id == request.VehicleId && v.ConnectUserId == userId,
                cancellationToken);
        if (vehicle is null) return null;

        var tenantExists = await db.Tenants
            .IgnoreQueryFilters()
            .AnyAsync(t => t.Id == request.TenantId && t.IsActive, cancellationToken);
        if (!tenantExists) return null;

        // Look for a classified tenant Car matching this plate.
        var tenantCar = await db.Cars
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(c => c.TenantId == request.TenantId && c.PlateNumber == vehicle.PlateNumber)
            .Select(c => new { c.VehicleTypeId, c.SizeId })
            .FirstOrDefaultAsync(cancellationToken);

        var services = await db.Services
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == request.TenantId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Description,
                s.BasePrice,
            })
            .ToListAsync(cancellationToken);

        if (services.Count == 0)
        {
            var emptyMode = tenantCar is not null ? PriceModeExact : PriceModeEstimate;
            return new ConnectServicesWithPricingDto(
                request.TenantId, request.VehicleId, emptyMode, []);
        }

        var serviceIds = services.Select(s => s.Id).ToList();

        var pricingRows = await db.ServicePricings
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(p => p.TenantId == request.TenantId && serviceIds.Contains(p.ServiceId))
            .Select(p => new
            {
                p.ServiceId,
                p.VehicleTypeId,
                p.SizeId,
                p.Price,
            })
            .ToListAsync(cancellationToken);

        if (tenantCar is not null)
        {
            // Classified — return exact price per service using the matching
            // (VehicleTypeId, SizeId) cell, falling back to BasePrice.
            var exactLookup = pricingRows
                .Where(p => p.VehicleTypeId == tenantCar.VehicleTypeId
                         && p.SizeId == tenantCar.SizeId)
                .ToDictionary(p => p.ServiceId, p => p.Price);

            var exactItems = services.Select(s =>
            {
                var price = exactLookup.TryGetValue(s.Id, out var p) ? p : s.BasePrice;
                return new ConnectServicePriceDto(
                    s.Id,
                    s.Name,
                    s.Description,
                    PriceModeExact,
                    Price: price,
                    PriceMin: null,
                    PriceMax: null);
            }).ToList();

            return new ConnectServicesWithPricingDto(
                request.TenantId, request.VehicleId, PriceModeExact, exactItems);
        }

        // Unclassified — return min/max across all pricing rows. When the matrix
        // is empty for a service we show BasePrice as both min and max.
        var groupedPricing = pricingRows
            .GroupBy(p => p.ServiceId)
            .ToDictionary(
                g => g.Key,
                g => (Min: g.Min(x => x.Price), Max: g.Max(x => x.Price)));

        var estimateItems = services.Select(s =>
        {
            if (groupedPricing.TryGetValue(s.Id, out var range))
            {
                return new ConnectServicePriceDto(
                    s.Id,
                    s.Name,
                    s.Description,
                    PriceModeEstimate,
                    Price: null,
                    PriceMin: range.Min,
                    PriceMax: range.Max);
            }

            return new ConnectServicePriceDto(
                s.Id,
                s.Name,
                s.Description,
                PriceModeEstimate,
                Price: null,
                PriceMin: s.BasePrice,
                PriceMax: s.BasePrice);
        }).ToList();

        return new ConnectServicesWithPricingDto(
            request.TenantId, request.VehicleId, PriceModeEstimate, estimateItems);
    }
}
