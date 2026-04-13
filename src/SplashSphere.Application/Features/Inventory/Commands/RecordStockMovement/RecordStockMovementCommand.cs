using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Inventory.Commands.RecordStockMovement;

public sealed record RecordStockMovementCommand(
    string SupplyItemId,
    MovementType Type,
    decimal Quantity,
    decimal? UnitCost = null,
    string? Reference = null,
    string? Notes = null) : ICommand<string>;
