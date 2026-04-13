using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.UpdateSupplier;

public sealed record UpdateSupplierCommand(
    string Id,
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address,
    bool IsActive) : ICommand;
