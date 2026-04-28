using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Settings.Queries.GetReceiptSetting;

/// <summary>
/// Receipt-design configuration for a tenant. Mirrors
/// <see cref="Domain.Entities.ReceiptSetting"/>; <c>BranchId</c> is null for
/// the tenant default and non-null for a per-branch override (slice 4).
/// </summary>
public sealed record ReceiptSettingDto(
    string? BranchId,

    // Header
    bool ShowLogo,
    LogoSize LogoSize,
    LogoPosition LogoPosition,
    bool ShowBusinessName,
    bool ShowTagline,
    bool ShowBranchName,
    bool ShowBranchAddress,
    bool ShowBranchContact,
    bool ShowTIN,
    string? CustomHeaderText,

    // Body
    bool ShowServiceDuration,
    bool ShowEmployeeNames,
    bool ShowVehicleInfo,
    bool ShowDiscountBreakdown,
    bool ShowTaxLine,
    bool ShowTransactionNumber,
    bool ShowDateTime,
    bool ShowCashierName,

    // Customer
    bool ShowCustomerName,
    bool ShowCustomerPhone,
    bool ShowLoyaltyPointsEarned,
    bool ShowLoyaltyBalance,
    bool ShowLoyaltyTier,

    // Footer
    string ThankYouMessage,
    string? PromoText,
    bool ShowSocialMedia,
    bool ShowGCashQr,
    bool ShowGCashNumber,
    string? CustomFooterText,

    // Format
    ReceiptWidth ReceiptWidth,
    ReceiptFontSize FontSize,
    bool AutoCutPaper);
