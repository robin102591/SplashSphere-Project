using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Settings.Queries.GetDisplaySetting;

/// <summary>
/// Customer-display configuration for a tenant. Mirrors
/// <see cref="Domain.Entities.DisplaySetting"/>; <c>BranchId</c> is null for
/// the tenant default and non-null for a per-branch override.
/// </summary>
public sealed record DisplaySettingDto(
    string? BranchId,

    // Idle
    bool ShowLogo,
    bool ShowBusinessName,
    bool ShowTagline,
    bool ShowDateTime,
    bool ShowGCashQr,
    bool ShowSocialMedia,
    IReadOnlyList<string> PromoMessages,
    int PromoRotationSeconds,

    // Building / transaction
    bool ShowVehicleInfo,
    bool ShowCustomerName,
    bool ShowLoyaltyTier,
    bool ShowDiscountBreakdown,
    bool ShowTaxLine,

    // Completion
    bool ShowPaymentMethod,
    bool ShowChangeAmount,
    bool ShowPointsEarned,
    bool ShowPointsBalance,
    bool ShowThankYouMessage,
    bool ShowPromoText,
    int CompletionHoldSeconds,

    // Appearance
    DisplayTheme Theme,
    DisplayFontSize FontSize,
    DisplayOrientation Orientation);
