using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Events;

/// <summary>
/// Raised when a cashier opens a new shift.
/// Consumed by: SignalR hub (broadcasts <c>ShiftUpdated</c> to the branch group).
/// </summary>
public sealed record ShiftOpenedEvent(
    string ShiftId,
    string CashierId,
    string BranchId,
    string TenantId,
    decimal OpeningFund
) : DomainEventBase;

/// <summary>
/// Raised when a cashier closes a shift after submitting the denomination count.
/// Consumed by: SignalR hub (broadcasts <c>ShiftUpdated</c> to the branch group).
/// Admin is notified of auto-flagged shifts via <see cref="ShiftFlaggedEvent"/>.
/// </summary>
public sealed record ShiftClosedEvent(
    string ShiftId,
    string CashierId,
    string BranchId,
    string TenantId,
    decimal Variance,
    ReviewStatus AutoReviewStatus
) : DomainEventBase;

/// <summary>
/// Raised when a manager flags a closed shift for investigation.
/// Consumed by: SignalR hub (broadcasts <c>ShiftFlagged</c> to the tenant group
/// so that all admin sessions are notified).
/// </summary>
public sealed record ShiftFlaggedEvent(
    string ShiftId,
    string CashierId,
    string BranchId,
    string TenantId,
    decimal Variance,
    string FlaggedByUserId,
    string Notes
) : DomainEventBase;
