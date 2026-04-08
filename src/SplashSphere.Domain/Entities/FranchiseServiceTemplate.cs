namespace SplashSphere.Domain.Entities;

/// <summary>
/// Standard service template that a Franchisor pushes to Franchisees.
/// Tenant-scoped to the franchisor (global query filter applies).
/// </summary>
public sealed class FranchiseServiceTemplate : IAuditableEntity
{
    private FranchiseServiceTemplate() { } // EF Core

    public FranchiseServiceTemplate(string franchisorTenantId, string serviceName, decimal basePrice)
    {
        Id = Guid.NewGuid().ToString();
        FranchisorTenantId = franchisorTenantId;
        ServiceName = serviceName;
        BasePrice = basePrice;
    }

    public string Id { get; set; } = string.Empty;
    public string FranchisorTenantId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CategoryName { get; set; }
    public decimal BasePrice { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsRequired { get; set; }
    public string? PricingMatrixJson { get; set; }
    public string? CommissionMatrixJson { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────
    public Tenant FranchisorTenant { get; set; } = null!;
}
