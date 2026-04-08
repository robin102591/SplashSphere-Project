using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// Monthly royalty calculation for a franchisee.
/// Excluded from global tenant query filters (cross-tenant).
/// </summary>
public sealed class RoyaltyPeriod : IAuditableEntity
{
    private RoyaltyPeriod() { } // EF Core

    public RoyaltyPeriod(string franchisorTenantId, string franchiseeTenantId, string agreementId)
    {
        Id = Guid.NewGuid().ToString();
        FranchisorTenantId = franchisorTenantId;
        FranchiseeTenantId = franchiseeTenantId;
        AgreementId = agreementId;
    }

    public string Id { get; set; } = string.Empty;
    public string FranchisorTenantId { get; set; } = string.Empty;
    public string FranchiseeTenantId { get; set; } = string.Empty;
    public string AgreementId { get; set; } = string.Empty;

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal RoyaltyRate { get; set; }
    public decimal RoyaltyAmount { get; set; }
    public decimal MarketingFeeRate { get; set; }
    public decimal MarketingFeeAmount { get; set; }
    public decimal TechnologyFeeRate { get; set; }
    public decimal TechnologyFeeAmount { get; set; }
    public decimal TotalDue { get; set; }

    public RoyaltyStatus Status { get; set; } = RoyaltyStatus.Pending;
    public DateTime? PaidDate { get; set; }
    public string? PaymentReference { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────
    public Tenant FranchisorTenant { get; set; } = null!;
    public Tenant FranchiseeTenant { get; set; } = null!;
    public FranchiseAgreement Agreement { get; set; } = null!;
}
