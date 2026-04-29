using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Settings.Commands.UpdateDisplaySetting;

/// <summary>
/// Upserts a display-setting row for the current tenant. Pass
/// <c>BranchId = null</c> for the tenant default. Branch overrides reuse the
/// same command (gated by plan in the handler).
/// </summary>
public sealed record UpdateDisplaySettingCommand(
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
    DisplayOrientation Orientation) : ICommand;
