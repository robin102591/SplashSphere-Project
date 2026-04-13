using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Queries.GetPurchaseOrders;

public sealed class GetPurchaseOrdersQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPurchaseOrdersQuery, PagedResult<PurchaseOrderDto>>
{
    public async Task<PagedResult<PurchaseOrderDto>> Handle(
        GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = db.PurchaseOrders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SupplierId))
            query = query.Where(po => po.SupplierId == request.SupplierId);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            query = query.Where(po => po.BranchId == request.BranchId);

        if (request.Status.HasValue)
            query = query.Where(po => po.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(po => po.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(po => new PurchaseOrderDto(
                po.Id,
                po.PoNumber,
                po.Supplier.Name,
                po.Branch.Name,
                po.Status.ToString(),
                po.TotalAmount,
                po.OrderDate,
                po.ExpectedDeliveryDate,
                po.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<PurchaseOrderDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
