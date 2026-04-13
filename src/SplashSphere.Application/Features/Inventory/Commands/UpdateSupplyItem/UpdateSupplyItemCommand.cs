using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.UpdateSupplyItem;

public sealed record UpdateSupplyItemCommand(
    string Id,
    string Name,
    string Unit,
    string? CategoryId = null,
    string? Description = null,
    decimal? ReorderLevel = null,
    bool IsActive = true) : ICommand;
