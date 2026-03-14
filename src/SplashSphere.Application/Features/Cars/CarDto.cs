namespace SplashSphere.Application.Features.Cars;

public sealed record CarDto(
    string Id,
    string PlateNumber,
    string VehicleTypeId,
    string VehicleTypeName,
    string SizeId,
    string SizeName,
    string? MakeId,
    string? MakeName,
    string? ModelId,
    string? ModelName,
    string? CustomerId,
    string? CustomerFullName,
    string? Color,
    int? Year,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt);
