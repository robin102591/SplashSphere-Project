namespace SplashSphere.Domain.Entities;

/// <summary>
/// Per-tenant configuration that drives what appears on a printed / digital
/// receipt. A row with <see cref="BranchId"/> = <c>null</c> is the tenant
/// default; future per-branch overrides will live as additional rows with a
/// non-null <see cref="BranchId"/>.
/// <para>
/// Resolution rule (slice 4 will add the branch path):
/// 1. Branch-specific row (BranchId == requestedBranchId) → use it.
/// 2. Tenant default row (BranchId == null) → fallback.
/// 3. Neither exists → handler creates the default on read.
/// </para>
/// </summary>
public sealed class ReceiptSetting : IAuditableEntity, ITenantScoped
{
    private ReceiptSetting() { } // EF Core

    public ReceiptSetting(string tenantId, string? branchId = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        BranchId = branchId;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Null = tenant-default. Non-null = override for a specific branch.</summary>
    public string? BranchId { get; set; }

    // ── Header ───────────────────────────────────────────────────────────────
    public bool ShowLogo { get; set; } = true;
    public LogoSize LogoSize { get; set; } = LogoSize.Medium;
    public LogoPosition LogoPosition { get; set; } = LogoPosition.Center;
    public bool ShowBusinessName { get; set; } = true;
    public bool ShowTagline { get; set; } = true;
    public bool ShowBranchName { get; set; } = true;
    public bool ShowBranchAddress { get; set; } = true;
    public bool ShowBranchContact { get; set; } = true;
    public bool ShowTIN { get; set; }
    public string? CustomHeaderText { get; set; }

    // ── Body ─────────────────────────────────────────────────────────────────
    public bool ShowServiceDuration { get; set; } = true;
    public bool ShowEmployeeNames { get; set; } = true;
    public bool ShowVehicleInfo { get; set; } = true;
    public bool ShowDiscountBreakdown { get; set; } = true;
    public bool ShowTaxLine { get; set; }
    public bool ShowTransactionNumber { get; set; } = true;
    public bool ShowDateTime { get; set; } = true;
    public bool ShowCashierName { get; set; } = true;

    // ── Customer ─────────────────────────────────────────────────────────────
    public bool ShowCustomerName { get; set; } = true;
    public bool ShowCustomerPhone { get; set; }
    public bool ShowLoyaltyPointsEarned { get; set; } = true;
    public bool ShowLoyaltyBalance { get; set; } = true;
    public bool ShowLoyaltyTier { get; set; } = true;

    // ── Footer ───────────────────────────────────────────────────────────────
    public string ThankYouMessage { get; set; } = "Thank you for your patronage!";
    public string? PromoText { get; set; }
    public bool ShowSocialMedia { get; set; } = true;
    public bool ShowGCashQr { get; set; }
    public bool ShowGCashNumber { get; set; }
    public string? CustomFooterText { get; set; }

    // ── Format ───────────────────────────────────────────────────────────────
    public ReceiptWidth ReceiptWidth { get; set; } = ReceiptWidth.Mm58;
    public ReceiptFontSize FontSize { get; set; } = ReceiptFontSize.Normal;
    public bool AutoCutPaper { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────
    public Tenant Tenant { get; set; } = null!;
    public Branch? Branch { get; set; }
}
