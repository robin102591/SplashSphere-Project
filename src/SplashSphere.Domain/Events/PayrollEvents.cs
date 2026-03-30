namespace SplashSphere.Domain.Events;

/// <summary>
/// Raised when a payroll period transitions from <c>Open</c> → <c>Closed</c>.
/// Triggers <see cref="PayrollEntry"/> creation per employee (commissions summed,
/// attendance counted, base salary computed).
/// Consumed by: payroll close handler, admin notification.
/// </summary>
public sealed record PayrollPeriodClosedEvent(
    string PayrollPeriodId,
    string TenantId,
    string? BranchId,
    int Year,
    int CutOffWeek,
    DateOnly StartDate,
    DateOnly EndDate,
    int EntryCount
) : DomainEventBase;

/// <summary>
/// Raised when a payroll period transitions from <c>Closed</c> → <c>Processed</c>.
/// The period becomes fully immutable after this event.
/// Consumed by: admin notification, audit log.
/// </summary>
public sealed record PayrollProcessedEvent(
    string PayrollPeriodId,
    string TenantId,
    string? BranchId,
    int Year,
    int CutOffWeek,
    /// <summary>Sum of all <c>PayrollEntry.NetPay</c> values in the period.</summary>
    decimal TotalNetPay
) : DomainEventBase;

/// <summary>
/// Raised when a payroll period transitions from <c>Processed</c> → <c>Released</c>.
/// Pay has been disbursed to employees. Terminal state.
/// </summary>
public sealed record PayrollReleasedEvent(
    string PayrollPeriodId,
    string TenantId,
    string? BranchId,
    int Year,
    int CutOffWeek,
    decimal TotalNetPay,
    int EntryCount
) : DomainEventBase;
