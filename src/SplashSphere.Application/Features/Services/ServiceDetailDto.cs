using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Services;

public sealed record ServiceDetailDto(
    string Id,
    string Name,
    string? Description,
    decimal BasePrice,
    string CategoryId,
    string CategoryName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ServicePricingRowDto> Pricing,
    IReadOnlyList<ServiceCommissionRowDto> Commissions);

public sealed record ServicePricingRowDto(
    string Id,
    string VehicleTypeId,
    string VehicleTypeName,
    string SizeId,
    string SizeName,
    decimal Price);

public sealed record ServiceCommissionRowDto(
    string Id,
    string VehicleTypeId,
    string VehicleTypeName,
    string SizeId,
    string SizeName,
    CommissionType Type,
    decimal? FixedAmount,
    decimal? PercentageRate);
