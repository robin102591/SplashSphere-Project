namespace SplashSphere.Application.Features.Services;

public sealed record ServiceSummaryDto(
    string Id,
    string Name,
    string? Description,
    decimal BasePrice,
    string CategoryId,
    string CategoryName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
