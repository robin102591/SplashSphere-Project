namespace SplashSphere.Application.Features.Makes;

public sealed record MakeDto(
    string Id,
    string Name,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
