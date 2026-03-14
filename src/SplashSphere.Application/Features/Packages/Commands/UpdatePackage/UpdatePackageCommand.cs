using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Packages.Commands.UpdatePackage;

/// <summary>
/// Updates package metadata and replaces the included services list.
/// The entire service list is replaced — send the complete desired set of ServiceIds.
/// </summary>
public sealed record UpdatePackageCommand(
    string Id,
    string Name,
    string? Description,
    IReadOnlyList<string> ServiceIds) : ICommand;
