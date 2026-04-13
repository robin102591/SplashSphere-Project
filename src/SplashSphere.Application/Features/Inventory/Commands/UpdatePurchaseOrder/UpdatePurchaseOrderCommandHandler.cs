using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Commands.UpdatePurchaseOrder;

public sealed class UpdatePurchaseOrderCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdatePurchaseOrderCommand, Result>
{
    public async Task<Result> Handle(UpdatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await db.PurchaseOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (po is null)
            return Result.Failure(Error.NotFound("PurchaseOrder", request.Id));

        if (po.Status != PurchaseOrderStatus.Draft)
            return Result.Failure(Error.Validation("PurchaseOrder.NotDraft",
                "Only draft purchase orders can be modified."));

        // Remove existing lines
        db.PurchaseOrderLines.RemoveRange(po.Lines);

        // Add new lines
        decimal totalAmount = 0;
        foreach (var line in request.Lines)
        {
            var poLine = new PurchaseOrderLine(po.Id, line.ItemName, line.Quantity, line.UnitCost)
            {
                SupplyItemId = line.SupplyItemId,
                MerchandiseId = line.MerchandiseId,
            };

            totalAmount += poLine.TotalCost;
            db.PurchaseOrderLines.Add(poLine);
        }

        po.Notes = request.Notes;
        po.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        po.TotalAmount = totalAmount;

        return Result.Success();
    }
}
