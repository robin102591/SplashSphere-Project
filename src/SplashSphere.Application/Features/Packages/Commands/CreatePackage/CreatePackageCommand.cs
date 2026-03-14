using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Packages.Commands.CreatePackage;

/// <summary>
/// Creates a new package with its included services.
/// ServiceIds must reference active services that belong to the current tenant.
/// </summary>
public sealed record CreatePackageCommand(
    string Name,
    string? Description,
    IReadOnlyList<string> ServiceIds) : ICommand<string>;
