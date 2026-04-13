using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Inventory.Commands.CreatePurchaseOrder;

namespace SplashSphere.Application.Features.Inventory.Commands.UpdatePurchaseOrder;

public sealed record UpdatePurchaseOrderCommand(
    string Id,
    string? Notes,
    DateTime? ExpectedDeliveryDate,
    IReadOnlyList<CreatePurchaseOrderLineRequest> Lines) : ICommand;
