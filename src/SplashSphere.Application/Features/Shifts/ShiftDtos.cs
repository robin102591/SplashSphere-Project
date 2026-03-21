using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Shifts;

// ── Shift list / summary ──────────────────────────────────────────────────────

public sealed record ShiftSummaryDto(
    string Id,
    string BranchId,
    string BranchName,
    string CashierId,
    string CashierName,
    DateOnly ShiftDate,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    ShiftStatus Status,
    decimal OpeningCashFund,
    decimal TotalRevenue,
    decimal Variance,
    ReviewStatus ReviewStatus,
    string? ReviewedByName,
    DateTime? ReviewedAt);

// ── Full shift detail ─────────────────────────────────────────────────────────

public sealed record ShiftDetailDto(
    string Id,
    string BranchId,
    string BranchName,
    string CashierId,
    string CashierName,
    DateOnly ShiftDate,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    ShiftStatus Status,
    decimal OpeningCashFund,
    decimal TotalCashPayments,
    decimal TotalNonCashPayments,
    decimal TotalCashIn,
    decimal TotalCashOut,
    decimal ExpectedCashInDrawer,
    decimal ActualCashInDrawer,
    decimal Variance,
    int TotalTransactionCount,
    decimal TotalRevenue,
    decimal TotalCommissions,
    decimal TotalDiscounts,
    ReviewStatus ReviewStatus,
    string? ReviewedById,
    string? ReviewedByName,
    DateTime? ReviewedAt,
    string? ReviewNotes,
    IReadOnlyList<CashMovementDto> CashMovements,
    IReadOnlyList<ShiftDenominationDto> Denominations,
    IReadOnlyList<ShiftPaymentSummaryDto> PaymentSummaries);

// ── Cash movement ─────────────────────────────────────────────────────────────

public sealed record CashMovementDto(
    string Id,
    CashMovementType Type,
    decimal Amount,
    string Reason,
    string? Reference,
    DateTime MovementTime);

// ── Denomination ──────────────────────────────────────────────────────────────

public sealed record ShiftDenominationDto(
    decimal DenominationValue,
    int Count,
    decimal Subtotal);

// ── Payment summary ───────────────────────────────────────────────────────────

public sealed record ShiftPaymentSummaryDto(
    PaymentMethod Method,
    int TransactionCount,
    decimal TotalAmount);

// ── End-of-day report ─────────────────────────────────────────────────────────

public sealed record ShiftReportDto(
    ShiftDetailDto Shift,
    IReadOnlyList<TopServiceDto> TopServices,
    IReadOnlyList<TopEmployeeDto> TopEmployees,
    DateTime GeneratedAt);

public sealed record TopServiceDto(
    string ServiceName,
    int TransactionCount,
    decimal TotalAmount);

public sealed record TopEmployeeDto(
    string EmployeeId,
    string EmployeeName,
    int ServiceCount,
    decimal TotalCommission);

// ── Variance analysis ─────────────────────────────────────────────────────────

public sealed record ShiftVarianceCashierDto(
    string CashierId,
    string CashierName,
    int ShiftCount,
    decimal TotalVariance,
    decimal AverageVariance,
    decimal LargestShortage);

public sealed record VarianceTrendPointDto(
    DateOnly ShiftDate,
    decimal Variance,
    ReviewStatus ReviewStatus);
