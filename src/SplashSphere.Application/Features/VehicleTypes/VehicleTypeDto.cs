namespace SplashSphere.Application.Features.VehicleTypes;

public sealed record VehicleTypeDto(
    string Id,
    string Name,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
