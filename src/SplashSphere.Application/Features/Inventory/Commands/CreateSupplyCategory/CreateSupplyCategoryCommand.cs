using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Commands.CreateSupplyCategory;

public sealed record CreateSupplyCategoryCommand(string Name, string? Description = null) : ICommand<string>;
