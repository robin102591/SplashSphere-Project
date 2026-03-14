namespace SplashSphere.Application.Features.Packages;

public sealed record PackageSummaryDto(
    string Id,
    string Name,
    string? Description,
    int ServiceCount,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
