using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.CancelPurchaseOrder;

public sealed record CancelPurchaseOrderCommand(string Id) : ICommand;
