using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.CreateSupplyItem;

public sealed record CreateSupplyItemCommand(
    string BranchId,
    string Name,
    string Unit,
    string? CategoryId = null,
    string? Description = null,
    decimal? ReorderLevel = null,
    decimal InitialStock = 0,
    decimal InitialUnitCost = 0) : ICommand<string>;
