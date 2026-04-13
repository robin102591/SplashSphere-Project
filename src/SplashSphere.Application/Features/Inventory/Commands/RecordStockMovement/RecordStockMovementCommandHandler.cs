using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.RecordStockMovement;

public sealed class RecordStockMovementCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<RecordStockMovementCommand, Result<string>>
{
    private static readonly HashSet<MovementType> InTypes =
    [
        MovementType.PurchaseIn,
        MovementType.AdjustmentIn,
        MovementType.TransferIn,
        MovementType.ReturnIn,
    ];

    public async Task<Result<string>> Handle(
        RecordStockMovementCommand request,
        CancellationToken cancellationToken)
    {
        var item = await db.SupplyItems
            .FirstOrDefaultAsync(i => i.Id == request.SupplyItemId, cancellationToken);

        if (item is null)
            return Result.Failure<string>(Error.NotFound("SupplyItem", request.SupplyItemId));

        var movement = new StockMovement(
            tenantContext.TenantId,
            item.BranchId,
            request.Type,
            request.Quantity)
        {
            SupplyItemId = item.Id,
            UnitCost = request.UnitCost,
            TotalCost = request.UnitCost.HasValue ? request.Quantity * request.UnitCost.Value : null,
            Reference = request.Reference,
            Notes = request.Notes,
            PerformedByUserId = tenantContext.UserId,
        };

        // Update stock
        if (InTypes.Contains(request.Type))
        {
            // Recalculate weighted average cost on PurchaseIn
            if (request.Type == MovementType.PurchaseIn && request.UnitCost.HasValue)
            {
                var existingStock = item.CurrentStock;
                var existingCost = item.AverageUnitCost;
                var newTotal = existingStock * existingCost + request.Quantity * request.UnitCost.Value;
                var newStock = existingStock + request.Quantity;
                item.AverageUnitCost = newStock > 0
                    ? Math.Round(newTotal / newStock, 2, MidpointRounding.AwayFromZero)
                    : 0;
            }

            item.CurrentStock += request.Quantity;
        }
        else
        {
            item.CurrentStock -= request.Quantity;
        }

        db.StockMovements.Add(movement);
        return Result.Success(movement.Id);
    }
}
