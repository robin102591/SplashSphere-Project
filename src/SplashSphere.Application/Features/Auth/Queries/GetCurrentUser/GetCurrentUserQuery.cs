using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Auth.Queries.GetCurrentUser;

/// <summary>
/// Returns the authenticated user's profile and linked tenant.
/// Works for users with no tenant (returns null Tenant).
/// Bypasses the global TenantId query filter — clerkUserId is cross-tenant.
/// </summary>
public sealed record GetCurrentUserQuery : IQuery<CurrentUserDto?>;
