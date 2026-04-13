using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.ReceivePurchaseOrder;

public sealed class ReceivePurchaseOrderCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<ReceivePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(ReceivePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await db.PurchaseOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (po is null)
            return Result.Failure(Error.NotFound("PurchaseOrder", request.Id));

        if (po.Status is not (PurchaseOrderStatus.Sent or PurchaseOrderStatus.PartiallyReceived))
            return Result.Failure(Error.Validation("PurchaseOrder.InvalidStatus",
                "Only sent or partially received purchase orders can receive goods."));

        var lineDict = po.Lines.ToDictionary(l => l.Id);

        foreach (var receiveLine in request.Lines)
        {
            if (!lineDict.TryGetValue(receiveLine.LineId, out var poLine))
                return Result.Failure(Error.NotFound("PurchaseOrderLine", receiveLine.LineId));

            if (receiveLine.ReceivedQuantity <= 0)
                continue;

            poLine.ReceivedQuantity += receiveLine.ReceivedQuantity;

            // Update supply item stock
            if (poLine.SupplyItemId is not null)
            {
                var supplyItem = await db.SupplyItems
                    .FirstOrDefaultAsync(s => s.Id == poLine.SupplyItemId, cancellationToken);

                if (supplyItem is not null)
                {
                    // Weighted average cost: ((currentStock * avgCost) + (received * unitCost)) / newTotal
                    var totalCostBefore = supplyItem.CurrentStock * supplyItem.AverageUnitCost;
                    var incomingCost = receiveLine.ReceivedQuantity * poLine.UnitCost;
                    var newTotal = supplyItem.CurrentStock + receiveLine.ReceivedQuantity;

                    supplyItem.AverageUnitCost = newTotal > 0
                        ? Math.Round((totalCostBefore + incomingCost) / newTotal, 2, MidpointRounding.AwayFromZero)
                        : poLine.UnitCost;

                    supplyItem.CurrentStock += receiveLine.ReceivedQuantity;

                    var movement = new StockMovement(tenantContext.TenantId, po.BranchId, MovementType.PurchaseIn, receiveLine.ReceivedQuantity)
                    {
                        SupplyItemId = poLine.SupplyItemId,
                        UnitCost = poLine.UnitCost,
                        TotalCost = receiveLine.ReceivedQuantity * poLine.UnitCost,
                        Reference = po.PoNumber,
                    };

                    db.StockMovements.Add(movement);
                }
            }

            // Update merchandise stock
            if (poLine.MerchandiseId is not null)
            {
                var merchandise = await db.Merchandise
                    .FirstOrDefaultAsync(m => m.Id == poLine.MerchandiseId, cancellationToken);

                if (merchandise is not null)
                {
                    merchandise.StockQuantity += (int)receiveLine.ReceivedQuantity;

                    var movement = new StockMovement(tenantContext.TenantId, po.BranchId, MovementType.PurchaseIn, receiveLine.ReceivedQuantity)
                    {
                        MerchandiseId = poLine.MerchandiseId,
                        UnitCost = poLine.UnitCost,
                        TotalCost = receiveLine.ReceivedQuantity * poLine.UnitCost,
                        Reference = po.PoNumber,
                    };

                    db.StockMovements.Add(movement);
                }
            }
        }

        // Determine new PO status
        var allFullyReceived = po.Lines.All(l => l.ReceivedQuantity >= l.Quantity);
        po.Status = allFullyReceived ? PurchaseOrderStatus.Received : PurchaseOrderStatus.PartiallyReceived;

        return Result.Success();
    }
}
