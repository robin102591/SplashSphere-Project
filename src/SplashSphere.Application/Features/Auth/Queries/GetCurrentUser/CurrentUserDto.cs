namespace SplashSphere.Application.Features.Auth.Queries.GetCurrentUser;

public sealed record CurrentUserDto(
    string Id,
    string ClerkUserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string? Role,
    bool IsActive,
    CurrentUserTenantDto? Tenant);

public sealed record CurrentUserTenantDto(
    string Id,
    string Name,
    string Email,
    string ContactNumber,
    string Address,
    bool IsActive);
