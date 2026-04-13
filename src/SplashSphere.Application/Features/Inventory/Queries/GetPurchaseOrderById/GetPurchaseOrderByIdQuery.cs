using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetPurchaseOrderById;

public sealed record GetPurchaseOrderByIdQuery(string Id) : IQuery<PurchaseOrderDetailDto?>;
