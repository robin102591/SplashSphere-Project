using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Transactions.Queries.GetReceipt;

public sealed record ReceiptDto(
    string TransactionId,
    string TransactionNumber,
    DateTime IssuedAt,
    ReceiptCompanyDto Company,
    ReceiptBranchDto Branch,
    ReceiptVehicleDto Vehicle,
    ReceiptCustomerDto? Customer,
    string CashierName,
    IReadOnlyList<ReceiptLineItemDto> LineItems,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    IReadOnlyList<ReceiptPaymentDto> Payments,
    string? Notes,
    ReceiptSettingsDto Settings);

/// <summary>
/// Tenant-level branding flowed onto every receipt. Lives next to
/// <see cref="ReceiptBranchDto"/> so the PDF renderer can compose a
/// "Business name + Branch" header without two parallel lookups.
/// </summary>
public sealed record ReceiptCompanyDto(
    string BusinessName,
    string? Tagline,
    string? TaxId,
    bool IsVatRegistered,
    string? FacebookUrl,
    string? InstagramHandle,
    string? GCashNumber,
    string? LogoThumbnailUrl);

public sealed record ReceiptBranchDto(
    string Id,
    string Name,
    string Address,
    string ContactNumber);

public sealed record ReceiptVehicleDto(
    string PlateNumber,
    string VehicleTypeName,
    string SizeName,
    string? MakeName,
    string? ModelName,
    string? Color,
    int? Year);

public sealed record ReceiptCustomerDto(
    string Id,
    string Name,
    string? ContactNumber);

public sealed record ReceiptLineItemDto(
    ReceiptLineType Type,
    string Name,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    IReadOnlyList<string> AssignedEmployees);

public sealed record ReceiptPaymentDto(
    PaymentMethod Method,
    decimal Amount,
    string? Reference,
    DateTime PaidAt);

public enum ReceiptLineType { Service, Package, Merchandise }

/// <summary>
/// Subset of <see cref="Domain.Entities.ReceiptSetting"/> propagated to the
/// PDF renderer. Defaults are applied at the handler layer when no setting
/// row exists, so consumers always get a populated value.
/// </summary>
public sealed record ReceiptSettingsDto(
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
    ReceiptFontSize FontSize);
