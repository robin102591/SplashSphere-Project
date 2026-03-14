namespace SplashSphere.Application.Features.Models;

public sealed record ModelDto(
    string Id,
    string MakeId,
    string MakeName,
    string Name,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
