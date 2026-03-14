namespace SplashSphere.Application.Features.Sizes;

public sealed record SizeDto(
    string Id,
    string Name,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
