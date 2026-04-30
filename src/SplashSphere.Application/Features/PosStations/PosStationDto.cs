namespace SplashSphere.Application.Features.PosStations;

public sealed record PosStationDto(
    string Id,
    string BranchId,
    string Name,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
