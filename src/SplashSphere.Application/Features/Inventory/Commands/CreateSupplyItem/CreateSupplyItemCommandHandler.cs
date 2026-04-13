using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.CreateSupplyItem;

public sealed class CreateSupplyItemCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<CreateSupplyItemCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateSupplyItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = new SupplyItem(
            tenantContext.TenantId,
            request.BranchId,
            request.Name,
            request.Unit,
            request.CategoryId,
            request.ReorderLevel)
        {
            Description = request.Description,
        };

        if (request.InitialStock > 0)
        {
            item.CurrentStock = request.InitialStock;
            item.AverageUnitCost = request.InitialUnitCost;

            var movement = new StockMovement(
                tenantContext.TenantId,
                request.BranchId,
                MovementType.PurchaseIn,
                request.InitialStock)
            {
                SupplyItemId = item.Id,
                UnitCost = request.InitialUnitCost,
                TotalCost = request.InitialStock * request.InitialUnitCost,
                PerformedByUserId = tenantContext.UserId,
                Notes = "Initial stock",
            };

            db.StockMovements.Add(movement);
        }

        db.SupplyItems.Add(item);
        return Result.Success(item.Id);
    }
}
