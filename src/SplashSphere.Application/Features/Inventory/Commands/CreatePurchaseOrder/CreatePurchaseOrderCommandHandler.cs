using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<CreatePurchaseOrderCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreatePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"PO-{year}-";

        var lastPoNumber = await db.PurchaseOrders
            .AsNoTracking()
            .Where(po => po.PoNumber.StartsWith(prefix))
            .OrderByDescending(po => po.PoNumber)
            .Select(po => po.PoNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = 1;
        if (lastPoNumber is not null)
        {
            var lastSeqStr = lastPoNumber[(prefix.Length)..];
            if (int.TryParse(lastSeqStr, out var lastSeq))
                sequence = lastSeq + 1;
        }

        var poNumber = $"{prefix}{sequence:D4}";

        var po = new PurchaseOrder(tenantContext.TenantId, request.BranchId, request.SupplierId, poNumber)
        {
            Status = PurchaseOrderStatus.Draft,
            OrderDate = DateTime.UtcNow,
            Notes = request.Notes,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
        };

        decimal totalAmount = 0;

        foreach (var line in request.Lines)
        {
            var poLine = new PurchaseOrderLine(po.Id, line.ItemName, line.Quantity, line.UnitCost)
            {
                SupplyItemId = line.SupplyItemId,
                MerchandiseId = line.MerchandiseId,
            };

            totalAmount += poLine.TotalCost;
            po.Lines.Add(poLine);
        }

        po.TotalAmount = totalAmount;

        db.PurchaseOrders.Add(po);
        return Result.Success(po.Id);
    }
}
