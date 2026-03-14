using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Packages.Commands.UpsertPackageCommission;

/// <summary>
/// Atomically replaces the commission matrix for a package.
/// Package commissions are always percentage-based — no Type enum or FixedAmount.
/// An empty Rows list clears the matrix (₱0 commission for all lookups).
/// </summary>
public sealed record UpsertPackageCommissionCommand(
    string PackageId,
    IReadOnlyList<PackageCommissionRowRequest> Rows) : ICommand;

public sealed record PackageCommissionRowRequest(
    string VehicleTypeId,
    string SizeId,
    decimal PercentageRate);
