using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Payroll;

// ── Period list ───────────────────────────────────────────────────────────────

public sealed record PayrollPeriodSummaryDto(
    string Id,
    PayrollStatus Status,
    int Year,
    int CutOffWeek,
    DateOnly StartDate,
    DateOnly EndDate,
    int EntryCount,
    decimal TotalNetPay,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// ── Period detail with entries ────────────────────────────────────────────────

public sealed record PayrollPeriodDetailDto(
    string Id,
    PayrollStatus Status,
    int Year,
    int CutOffWeek,
    DateOnly StartDate,
    DateOnly EndDate,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<PayrollEntryDto> Entries);

public sealed record PayrollEntryDto(
    string Id,
    string EmployeeId,
    string EmployeeName,
    string BranchName,
    EmployeeType EmployeeTypeSnapshot,
    int DaysWorked,
    decimal? DailyRateSnapshot,
    decimal BaseSalary,
    decimal TotalCommissions,
    decimal Bonuses,
    decimal Deductions,
    decimal NetPay,
    string? Notes);

// ── Entry detail (drill-down) ───────────────────────────────────────────────

public sealed record PayrollEntryDetailDto(
    PayrollEntryDto Entry,
    IReadOnlyList<CommissionLineItemDto> CommissionLineItems,
    IReadOnlyList<AttendanceLineItemDto> AttendanceRecords);

public sealed record CommissionLineItemDto(
    string TransactionNumber,
    string ServiceName,
    decimal CommissionAmount,
    DateTime CompletedAt);

public sealed record AttendanceLineItemDto(
    DateOnly Date,
    DateTime TimeIn,
    DateTime? TimeOut);
