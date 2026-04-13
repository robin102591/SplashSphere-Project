using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetPurchaseOrderById;

public sealed class GetPurchaseOrderByIdQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDetailDto?>
{
    public async Task<PurchaseOrderDetailDto?> Handle(
        GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.PurchaseOrders
            .AsNoTracking()
            .Where(po => po.Id == request.Id)
            .Select(po => new PurchaseOrderDetailDto(
                po.Id,
                po.PoNumber,
                po.SupplierId,
                po.Supplier.Name,
                po.BranchId,
                po.Branch.Name,
                po.Status.ToString(),
                po.TotalAmount,
                po.Notes,
                po.OrderDate,
                po.ExpectedDeliveryDate,
                po.CreatedAt,
                po.Lines.Select(l => new PurchaseOrderLineDto(
                    l.Id,
                    l.ItemName,
                    l.SupplyItemId,
                    l.MerchandiseId,
                    l.Quantity,
                    l.ReceivedQuantity,
                    l.UnitCost,
                    l.TotalCost)).ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
