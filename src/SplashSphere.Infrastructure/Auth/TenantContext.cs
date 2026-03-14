using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Infrastructure.Auth;

public sealed class TenantContext : ITenantContext
{
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ClerkUserId { get; set; } = string.Empty;
    public string? BranchId { get; set; }
    public string? Role { get; set; }
}
