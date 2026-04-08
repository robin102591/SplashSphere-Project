namespace SplashSphere.Domain.Entities;

/// <summary>
/// Token-based invitation from a Franchisor to a new Franchisee.
/// Excluded from global tenant query filters (public access for validation).
/// </summary>
public sealed class FranchiseInvitation : IAuditableEntity
{
    private FranchiseInvitation() { } // EF Core

    public FranchiseInvitation(string franchisorTenantId, string email, string businessName, string token)
    {
        Id = Guid.NewGuid().ToString();
        FranchisorTenantId = franchisorTenantId;
        Email = email;
        BusinessName = businessName;
        Token = token;
    }

    public string Id { get; set; } = string.Empty;
    public string FranchisorTenantId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public string? FranchiseCode { get; set; }
    public string? TerritoryName { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public string? AcceptedByTenantId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────
    public Tenant FranchisorTenant { get; set; } = null!;
}
