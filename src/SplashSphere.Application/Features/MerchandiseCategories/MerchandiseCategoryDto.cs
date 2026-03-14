namespace SplashSphere.Application.Features.MerchandiseCategories;

public sealed record MerchandiseCategoryDto(
    string Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
