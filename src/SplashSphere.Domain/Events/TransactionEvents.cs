namespace SplashSphere.Domain.Events;

/// <summary>
/// Raised after a transaction is fully persisted to the database.
/// Consumed by: SignalR hub (broadcasts <c>TransactionUpdated</c> to branch group),
/// dashboard metrics update, and — when <see cref="QueueEntryId"/> is set —
/// the queue state machine (sets queue entry to <c>InService</c>).
/// </summary>
public sealed record TransactionCreatedEvent(
    string TransactionId,
    string TenantId,
    string BranchId,
    string TransactionNumber,
    string CarId,
    decimal FinalAmount,
    TransactionStatus Status,
    string? CustomerId = null,
    string? QueueEntryId = null
) : DomainEventBase;

/// <summary>
/// Raised whenever a transaction's status changes.
/// Consumed by: SignalR hub, and — when <see cref="QueueEntryId"/> is set —
/// the queue state machine to propagate matching status changes (e.g. Completed → queue Completed).
/// </summary>
public sealed record TransactionStatusChangedEvent(
    string TransactionId,
    string TenantId,
    string BranchId,
    TransactionStatus PreviousStatus,
    TransactionStatus NewStatus,
    string? QueueEntryId = null
) : DomainEventBase;

/// <summary>
/// Raised when a transaction reaches <see cref="TransactionStatus.Completed"/>.
/// Specialised event (subset of <see cref="TransactionStatusChangedEvent"/>) consumed by:
/// payroll commission accumulation, inventory decrement confirmation, receipt generation,
/// and SignalR <c>DashboardMetricsUpdated</c> broadcast.
/// </summary>
public sealed record TransactionCompletedEvent(
    string TransactionId,
    string TenantId,
    string BranchId,
    string TransactionNumber,
    decimal FinalAmount,
    string? QueueEntryId = null
) : DomainEventBase;
