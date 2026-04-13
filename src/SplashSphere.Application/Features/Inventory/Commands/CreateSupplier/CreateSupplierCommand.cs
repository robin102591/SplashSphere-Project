using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.CreateSupplier;

public sealed record CreateSupplierCommand(
    string Name,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address) : ICommand<string>;
