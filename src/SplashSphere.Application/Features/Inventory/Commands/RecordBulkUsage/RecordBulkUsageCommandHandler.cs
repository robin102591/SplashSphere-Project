using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.RecordBulkUsage;

public sealed class RecordBulkUsageCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<RecordBulkUsageCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        RecordBulkUsageCommand request,
        CancellationToken cancellationToken)
    {
        var supplyItemIds = request.Items.Select(i => i.SupplyItemId).Distinct().ToList();
        var items = await db.SupplyItems
            .Where(i => supplyItemIds.Contains(i.Id) && i.BranchId == request.BranchId)
            .ToDictionaryAsync(i => i.Id, cancellationToken);

        var count = 0;

        foreach (var entry in request.Items)
        {
            if (!items.TryGetValue(entry.SupplyItemId, out var supplyItem))
                continue;

            var movement = new StockMovement(
                tenantContext.TenantId,
                request.BranchId,
                MovementType.UsageOut,
                entry.Quantity)
            {
                SupplyItemId = supplyItem.Id,
                Notes = entry.Notes,
                PerformedByUserId = tenantContext.UserId,
            };

            supplyItem.CurrentStock -= entry.Quantity;
            db.StockMovements.Add(movement);
            count++;
        }

        return Result.Success(count);
    }
}
