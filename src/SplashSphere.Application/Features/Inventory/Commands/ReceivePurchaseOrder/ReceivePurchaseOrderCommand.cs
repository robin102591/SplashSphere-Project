using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.ReceivePurchaseOrder;

public sealed record ReceiveLineRequest(string LineId, decimal ReceivedQuantity);

public sealed record ReceivePurchaseOrderCommand(
    string Id,
    IReadOnlyList<ReceiveLineRequest> Lines) : ICommand;
