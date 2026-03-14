namespace SplashSphere.Application.Features.ServiceCategories;

public sealed record ServiceCategoryDto(
    string Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
