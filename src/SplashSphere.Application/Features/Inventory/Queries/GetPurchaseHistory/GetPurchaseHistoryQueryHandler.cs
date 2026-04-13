using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Inventory.Queries.GetPurchaseHistory;

public sealed class GetPurchaseHistoryQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPurchaseHistoryQuery, PurchaseHistoryDto>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<PurchaseHistoryDto> Handle(
        GetPurchaseHistoryQuery request, CancellationToken cancellationToken)
    {
        var fromUtc = DateTime.SpecifyKind(
            request.From.ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(
            request.To.AddDays(1).ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);

        // ── By Supplier (from PurchaseOrders) ────────────────────────────────────
        var poQuery = db.PurchaseOrders
            .AsNoTracking()
            .Where(po => (po.Status == PurchaseOrderStatus.Received
                          || po.Status == PurchaseOrderStatus.PartiallyReceived)
                         && po.CreatedAt >= fromUtc
                         && po.CreatedAt < toUtc);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            poQuery = poQuery.Where(po => po.BranchId == request.BranchId);

        var poRows = await poQuery
            .Select(po => new { SupplierName = po.Supplier.Name, po.TotalAmount })
            .ToListAsync(cancellationToken);

        var bySupplier = poRows
            .GroupBy(r => r.SupplierName)
            .Select(g => new PurchaseBySupplierDto(g.Key, g.Sum(r => r.TotalAmount), g.Count()))
            .OrderByDescending(s => s.Amount)
            .ToList();

        var totalSpending = poRows.Sum(r => r.TotalAmount);

        // ── By Category (from StockMovements PurchaseIn) ─────────────────────────
        var smQuery = db.StockMovements
            .AsNoTracking()
            .Where(sm => sm.Type == MovementType.PurchaseIn
                         && sm.MovementDate >= fromUtc
                         && sm.MovementDate < toUtc
                         && sm.SupplyItemId != null
                         && sm.TotalCost.HasValue);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            smQuery = smQuery.Where(sm => sm.BranchId == request.BranchId);

        var smRows = await smQuery
            .Select(sm => new
            {
                CategoryName = sm.SupplyItem!.Category != null
                    ? sm.SupplyItem.Category.Name
                    : "Uncategorised",
                TotalCost = sm.TotalCost!.Value,
            })
            .ToListAsync(cancellationToken);

        var byCategory = smRows
            .GroupBy(r => r.CategoryName)
            .Select(g => new PurchaseByCategoryDto(g.Key, g.Sum(r => r.TotalCost)))
            .OrderByDescending(c => c.Amount)
            .ToList();

        return new PurchaseHistoryDto(
            request.From, request.To,
            totalSpending,
            bySupplier,
            byCategory);
    }
}
