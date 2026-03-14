using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Packages.Commands.UpsertPackagePricing;

/// <summary>
/// Atomically replaces the pricing matrix for a package.
/// An empty Rows list clears the matrix.
/// </summary>
public sealed record UpsertPackagePricingCommand(
    string PackageId,
    IReadOnlyList<PackagePricingRowRequest> Rows) : ICommand;

public sealed record PackagePricingRowRequest(
    string VehicleTypeId,
    string SizeId,
    decimal Price);
