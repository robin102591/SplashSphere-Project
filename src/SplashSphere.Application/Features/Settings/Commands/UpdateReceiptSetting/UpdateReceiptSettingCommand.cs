using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Settings.Commands.UpdateReceiptSetting;

/// <summary>
/// Upserts a receipt setting row for the current tenant. Pass
/// <c>BranchId = null</c> for the tenant default (slice 2). Slice 4 wires
/// branch overrides (BranchId != null), reusing the same command.
/// </summary>
public sealed record UpdateReceiptSettingCommand(
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
    bool AutoCutPaper) : ICommand;
