using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.DeleteSupplyItem;

public sealed record DeleteSupplyItemCommand(string Id) : ICommand;
