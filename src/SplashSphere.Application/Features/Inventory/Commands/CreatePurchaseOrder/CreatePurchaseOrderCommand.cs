using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.CreatePurchaseOrder;

public sealed record CreatePurchaseOrderLineRequest(
    string? SupplyItemId,
    string? MerchandiseId,
    string ItemName,
    decimal Quantity,
    decimal UnitCost);

public sealed record CreatePurchaseOrderCommand(
    string SupplierId,
    string BranchId,
    string? Notes,
    DateTime? ExpectedDeliveryDate,
    IReadOnlyList<CreatePurchaseOrderLineRequest> Lines) : ICommand<string>;
