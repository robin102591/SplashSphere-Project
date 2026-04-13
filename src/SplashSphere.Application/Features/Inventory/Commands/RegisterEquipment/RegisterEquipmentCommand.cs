using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.RegisterEquipment;

public sealed record RegisterEquipmentCommand(
    string BranchId,
    string Name,
    string? Brand,
    string? Model,
    string? SerialNumber,
    DateTime? PurchaseDate,
    decimal? PurchaseCost,
    DateTime? WarrantyExpiry,
    string? Location,
    string? Notes) : ICommand<string>;
