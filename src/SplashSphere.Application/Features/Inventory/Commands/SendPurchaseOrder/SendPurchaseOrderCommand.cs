using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.SendPurchaseOrder;

public sealed record SendPurchaseOrderCommand(string Id) : ICommand;
