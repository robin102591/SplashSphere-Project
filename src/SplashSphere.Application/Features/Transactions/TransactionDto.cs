using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Transactions;

// ── Full transaction detail ───────────────────────────────────────────────────

public sealed record TransactionDetailDto(
    string Id,
    string TransactionNumber,
    string BranchId,
    string BranchName,
    string CarId,
    string PlateNumber,
    string VehicleTypeName,
    string VehicleTypeId,
    string SizeName,
    string SizeId,
    string? CustomerId,
    string? CustomerName,
    TransactionStatus Status,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal FinalAmount,
    decimal TipAmount,
    string CashierId,
    string CashierName,
    string? Notes,
    string? QueueEntryId,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt,
    DateTime? RefundedAt,
    string? RefundReason,
    int PointsEarned,
    IReadOnlyList<TransactionServiceLineDto> Services,
    IReadOnlyList<TransactionPackageLineDto> Packages,
    IReadOnlyList<TransactionMerchandiseLineDto> Merchandise,
    IReadOnlyList<TransactionEmployeeSummaryDto> Employees,
    IReadOnlyList<PaymentDto> Payments);

// ── Line items ────────────────────────────────────────────────────────────────

public sealed record TransactionServiceLineDto(
    string Id,
    string ServiceId,
    string ServiceName,
    string CategoryName,
    string VehicleTypeName,
    string SizeName,
    decimal UnitPrice,
    decimal TotalCommission,
    string? Notes,
    IReadOnlyList<ServiceAssignmentDto> EmployeeAssignments);

public sealed record TransactionPackageLineDto(
    string Id,
    string PackageId,
    string PackageName,
    string VehicleTypeName,
    string SizeName,
    decimal UnitPrice,
    decimal TotalCommission,
    string? Notes,
    IReadOnlyList<PackageAssignmentDto> EmployeeAssignments);

public sealed record TransactionMerchandiseLineDto(
    string Id,
    string MerchandiseId,
    string MerchandiseName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

// ── Employee summaries & assignments ─────────────────────────────────────────

public sealed record TransactionEmployeeSummaryDto(
    string Id,
    string EmployeeId,
    string EmployeeName,
    decimal TotalCommission);

public sealed record ServiceAssignmentDto(
    string Id,
    string EmployeeId,
    string EmployeeName,
    decimal CommissionAmount);

public sealed record PackageAssignmentDto(
    string Id,
    string EmployeeId,
    string EmployeeName,
    decimal CommissionAmount);

// ── Payments ──────────────────────────────────────────────────────────────────

public sealed record PaymentDto(
    string Id,
    PaymentMethod Method,
    decimal Amount,
    string? Reference,
    DateTime CreatedAt);
