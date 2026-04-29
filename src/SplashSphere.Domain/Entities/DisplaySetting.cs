using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Interfaces;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// Per-tenant configuration that drives what the customer-facing display
/// shows in each of its three states (Idle / Building / Complete) plus
/// appearance (theme, font size, orientation).
/// <para>
/// Resolution rule mirrors <see cref="ReceiptSetting"/>:
/// 1. Branch-specific row (BranchId == requestedBranchId) → use it.
/// 2. Tenant default row (BranchId == null) → fallback.
/// 3. Neither exists → handler returns in-memory defaults.
/// </para>
/// </summary>
public sealed class DisplaySetting : IAuditableEntity, ITenantScoped
{
    private DisplaySetting() { } // EF Core

    public DisplaySetting(string tenantId, string? branchId = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        BranchId = branchId;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Null = tenant-default. Non-null = override for a specific branch.</summary>
    public string? BranchId { get; set; }

    // ── Idle screen ──────────────────────────────────────────────────────────
    public bool ShowLogo { get; set; } = true;
    public bool ShowBusinessName { get; set; } = true;
    public bool ShowTagline { get; set; } = true;
    public bool ShowDateTime { get; set; } = true;
    public bool ShowGCashQr { get; set; }
    public bool ShowSocialMedia { get; set; }

    /// <summary>
    /// Promo messages rotated on the idle screen. Stored as a JSONB array of
    /// strings — small (typically 1-5 entries) and accessed atomically with
    /// the rest of the row, so a separate table would be overkill.
    /// </summary>
    public List<string> PromoMessages { get; set; } = [];

    public int PromoRotationSeconds { get; set; } = 8;

    // ── Building / transaction screen ────────────────────────────────────────
    public bool ShowVehicleInfo { get; set; } = true;
    public bool ShowCustomerName { get; set; } = true;
    public bool ShowLoyaltyTier { get; set; } = true;
    public bool ShowDiscountBreakdown { get; set; } = true;
    public bool ShowTaxLine { get; set; }

    // ── Completion screen ────────────────────────────────────────────────────
    public bool ShowPaymentMethod { get; set; } = true;
    public bool ShowChangeAmount { get; set; } = true;
    public bool ShowPointsEarned { get; set; } = true;
    public bool ShowPointsBalance { get; set; } = true;
    public bool ShowThankYouMessage { get; set; } = true;
    public bool ShowPromoText { get; set; } = true;

    /// <summary>
    /// How long the completion screen stays visible after payment before
    /// reverting to Idle. Customers usually walk away within 10 seconds, but
    /// busy counters may want shorter.
    /// </summary>
    public int CompletionHoldSeconds { get; set; } = 10;

    // ── Appearance ───────────────────────────────────────────────────────────
    public DisplayTheme Theme { get; set; } = DisplayTheme.Dark;
    public DisplayFontSize FontSize { get; set; } = DisplayFontSize.Large;
    public DisplayOrientation Orientation { get; set; } = DisplayOrientation.Landscape;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────
    public Tenant Tenant { get; set; } = null!;
    public Branch? Branch { get; set; }
}
