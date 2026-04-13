using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Queries.GetPurchaseOrders;

public sealed record GetPurchaseOrdersQuery(
    string? SupplierId = null,
    string? BranchId = null,
    PurchaseOrderStatus? Status = null,
    int Page = 1,
    int PageSize = 50) : IQuery<PagedResult<PurchaseOrderDto>>;
