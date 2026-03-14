namespace SplashSphere.Domain.Enums;

/// <summary>
/// Lifecycle states of a vehicle queue entry.
/// </summary>
/// <remarks>
/// Valid transitions:
/// <code>
/// Waiting → Called → InService → Completed
///    ↓         ↓
/// Cancelled  NoShow → (back to Waiting, or Cancelled)
/// </code>
/// </remarks>
public enum QueueStatus
{
    /// <summary>
    /// Vehicle has been added to the queue and is waiting to be called to a bay.
    /// This is the initial state for all new queue entries.
    /// </summary>
    Waiting = 1,

    /// <summary>
    /// The cashier has called the vehicle to a service bay.
    /// A 5-minute no-show timer is started via Hangfire when entering this state.
    /// </summary>
    Called = 2,

    /// <summary>
    /// The vehicle is in the bay and service is actively in progress.
    /// A linked transaction has been created and is <c>InProgress</c>.
    /// </summary>
    InService = 3,

    /// <summary>
    /// Service has been completed and payment collected.
    /// Terminal state — the queue entry is closed.
    /// </summary>
    Completed = 4,

    /// <summary>
    /// The queue entry was manually cancelled by staff before service began.
    /// Terminal state.
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// The customer failed to arrive at the bay within 5 minutes of being called.
    /// Can be re-queued (back to <c>Waiting</c>) or left as terminal.
    /// </summary>
    NoShow = 6,
}
