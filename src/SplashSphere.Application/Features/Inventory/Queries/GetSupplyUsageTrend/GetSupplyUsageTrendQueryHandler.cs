using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Inventory.Queries.GetSupplyUsageTrend;

public sealed class GetSupplyUsageTrendQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSupplyUsageTrendQuery, SupplyUsageTrendDto>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<SupplyUsageTrendDto> Handle(
        GetSupplyUsageTrendQuery request, CancellationToken cancellationToken)
    {
        var fromUtc = DateTime.SpecifyKind(
            request.From.ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(
            request.To.AddDays(1).ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);

        var query = db.StockMovements
            .AsNoTracking()
            .Where(sm => sm.Type == MovementType.UsageOut
                         && sm.MovementDate >= fromUtc
                         && sm.MovementDate < toUtc
                         && sm.SupplyItemId != null);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            query = query.Where(sm => sm.BranchId == request.BranchId);

        // Flat projection to avoid GroupBy translation issues
        var rows = await query
            .Select(sm => new
            {
                CategoryName = sm.SupplyItem!.Category != null
                    ? sm.SupplyItem.Category.Name
                    : "Uncategorised",
                sm.MovementDate,
                sm.Quantity,
                TotalCost = sm.TotalCost ?? 0m,
            })
            .ToListAsync(cancellationToken);

        // Group in memory by category and date
        var categories = rows
            .GroupBy(r => r.CategoryName)
            .OrderBy(g => g.Key)
            .Select(g => new UsageTrendCategoryDto(
                g.Key,
                g.GroupBy(r => DateOnly.FromDateTime(r.MovementDate.Add(ManilaOffset)))
                    .OrderBy(dg => dg.Key)
                    .Select(dg => new UsageTrendPointDto(
                        dg.Key,
                        dg.Sum(r => r.Quantity),
                        dg.Sum(r => r.TotalCost)))
                    .ToList()))
            .ToList();

        return new SupplyUsageTrendDto(request.From, request.To, categories);
    }
}
