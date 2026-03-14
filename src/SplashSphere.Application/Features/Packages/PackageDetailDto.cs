namespace SplashSphere.Application.Features.Packages;

public sealed record PackageDetailDto(
    string Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<PackageServiceDto> Services,
    IReadOnlyList<PackagePricingRowDto> Pricing,
    IReadOnlyList<PackageCommissionRowDto> Commissions);

/// <summary>One included service within a package.</summary>
public sealed record PackageServiceDto(
    string ServiceId,
    string ServiceName,
    string CategoryName);

/// <summary>One cell in the package pricing matrix.</summary>
public sealed record PackagePricingRowDto(
    string Id,
    string VehicleTypeId,
    string VehicleTypeName,
    string SizeId,
    string SizeName,
    decimal Price);

/// <summary>
/// One cell in the package commission matrix.
/// Package commissions are always percentage-based — no Type or FixedAmount.
/// </summary>
public sealed record PackageCommissionRowDto(
    string Id,
    string VehicleTypeId,
    string VehicleTypeName,
    string SizeId,
    string SizeName,
    decimal PercentageRate);
