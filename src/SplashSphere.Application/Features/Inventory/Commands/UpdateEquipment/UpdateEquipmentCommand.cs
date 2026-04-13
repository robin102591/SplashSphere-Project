using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.UpdateEquipment;

public sealed record UpdateEquipmentCommand(
    string Id,
    string Name,
    string? Brand,
    string? Model,
    string? SerialNumber,
    DateTime? PurchaseDate,
    decimal? PurchaseCost,
    DateTime? WarrantyExpiry,
    string? Location,
    string? Notes,
    bool IsActive) : ICommand;
