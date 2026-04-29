namespace SplashSphere.Application.Features.Display.DTOs;

/// <summary>
/// Customer-display-safe transaction projection. Used both as the SignalR
/// broadcast payload (Display Started / Updated) and as the response shape
/// for <c>GET /api/v1/display/current</c> (reconnect rehydration). Sharing
/// one type guarantees the frontend's reconnect-from-REST path matches the
/// SignalR-driven path byte-for-byte.
/// <para>
/// <b>Privacy</b>: never includes employee names, commission data, cost
/// prices, profit margins, or other internal fields.
/// </para>
/// </summary>
public sealed record DisplayTransactionResultDto(
    string TransactionId,

    // Vehicle (shown if available)
    string? VehiclePlate,
    string? VehicleMakeModel,
    string? VehicleTypeSize,

    // Customer (shown if linked + display setting allows)
    string? CustomerName,
    string? LoyaltyTier,

    IReadOnlyList<DisplayLineItemResultDto> Items,

    decimal Subtotal,
    decimal DiscountAmount,
    string? DiscountLabel,
    decimal TaxAmount,
    decimal Total);

public sealed record DisplayLineItemResultDto(
    string Id,
    string Name,
    string Type,        // "service" | "package" | "merchandise"
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

/// <summary>Completion-screen projection — wraps the transaction with payment + loyalty + footer fields.</summary>
public sealed record DisplayCompletionResultDto(
    DisplayTransactionResultDto Transaction,

    string PaymentMethod,
    decimal AmountPaid,
    decimal ChangeAmount,

    int? PointsEarned,
    int? PointsBalance,

    string? ThankYouMessage,
    string? PromoText);
