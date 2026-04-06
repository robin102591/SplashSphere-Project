namespace SplashSphere.Domain.Entities;

/// <summary>
/// Per-tenant loyalty program configuration (singleton per tenant, upsert pattern).
/// Controls how points are earned, whether they expire, and the master on/off switch.
/// </summary>
public sealed class LoyaltyProgramSettings : IAuditableEntity
{
    private LoyaltyProgramSettings() { } // EF Core

    public LoyaltyProgramSettings(string tenantId)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Points awarded per <see cref="CurrencyUnitAmount"/> pesos spent. Default 1.</summary>
    public decimal PointsPerCurrencyUnit { get; set; } = 1;

    /// <summary>The peso threshold per point award. Default 100 (1 point per P100).</summary>
    public decimal CurrencyUnitAmount { get; set; } = 100;

    /// <summary>Master switch for the loyalty program.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Months before earned points expire. Null means no expiry.</summary>
    public int? PointsExpirationMonths { get; set; }

    /// <summary>Whether to auto-enroll customers when they first complete a transaction.</summary>
    public bool AutoEnroll { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public ICollection<LoyaltyTierConfig> Tiers { get; set; } = [];
}
