namespace SplashSphere.Domain.Events;

/// <summary>
/// Raised when a vehicle is added to the queue.
/// Consumed by: SignalR hub (broadcasts <c>QueueUpdated</c> and <c>QueueDisplayUpdated</c>
/// to branch and public display groups).
/// </summary>
public sealed record QueueEntryCreatedEvent(
    string QueueEntryId,
    string TenantId,
    string BranchId,
    string QueueNumber,
    string PlateNumber,
    QueuePriority Priority,
    int? EstimatedWaitMinutes = null
) : DomainEventBase;

/// <summary>
/// Raised when a queue entry transitions to <see cref="QueueStatus.Called"/>.
/// Consumed by: SignalR hub (broadcasts <c>QueueUpdated</c> to branch group,
/// <c>QueueDisplayUpdated</c> to public display group), and the Hangfire no-show
/// scheduler (starts the 5-minute timer via <c>BackgroundJob.Schedule</c>).
/// </summary>
public sealed record QueueEntryCalledEvent(
    string QueueEntryId,
    string TenantId,
    string BranchId,
    string QueueNumber,
    string PlateNumber,
    DateTime CalledAt
) : DomainEventBase;

/// <summary>
/// Raised when a queue entry transitions to <see cref="QueueStatus.InService"/>.
/// Consumed by: SignalR hub (broadcasts <c>QueueUpdated</c> to branch group,
/// <c>QueueDisplayUpdated</c> to public display group).
/// </summary>
public sealed record QueueEntryInServiceEvent(
    string QueueEntryId,
    string TenantId,
    string BranchId,
    string QueueNumber,
    string PlateNumber,
    DateTime StartedAt
) : DomainEventBase;

/// <summary>
/// Raised by the Hangfire no-show job when a customer fails to arrive within
/// 5 minutes of being called and the entry is still in <see cref="QueueStatus.Called"/> state.
/// Consumed by: SignalR hub (broadcasts <c>QueueUpdated</c>), auto-call-next logic.
/// </summary>
public sealed record QueueEntryNoShowEvent(
    string QueueEntryId,
    string TenantId,
    string BranchId,
    string QueueNumber,
    string PlateNumber,
    DateTime NoShowAt
) : DomainEventBase;
