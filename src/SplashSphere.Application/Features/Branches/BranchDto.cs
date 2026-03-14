namespace SplashSphere.Application.Features.Branches;

public sealed record BranchDto(
    string Id,
    string Name,
    string Code,
    string Address,
    string ContactNumber,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
