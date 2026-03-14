namespace SplashSphere.Application.Common.Interfaces;

public interface ITenantContext
{
    string TenantId { get; set; }
    string UserId { get; set; }
    string ClerkUserId { get; set; }
    string? BranchId { get; set; }
    string? Role { get; set; }
}
