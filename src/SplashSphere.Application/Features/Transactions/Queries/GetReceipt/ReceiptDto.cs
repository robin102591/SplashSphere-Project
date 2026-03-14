using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Transactions.Queries.GetReceipt;

public sealed record ReceiptDto(
    string TransactionId,
    string TransactionNumber,
    DateTime IssuedAt,
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
    string? Notes);

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
