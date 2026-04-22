namespace SplashSphere.Domain.Events;

/// <summary>
/// Raised when a referral moves to <see cref="Enums.ReferralStatus.Completed"/>
/// — the referred customer has finished their first wash at the tenant and both
/// participants have had their points awarded.
/// Consumed by: notification + SMS handlers for admin/POS UI and customer outreach.
/// </summary>
public sealed record ReferralCompletedEvent(
    string ReferralId,
    string TenantId,
    string BranchId,
    string ReferrerCustomerId,
    string ReferredCustomerId,
    int ReferrerPointsAwarded,
    int ReferredPointsAwarded,
    string ReferralCode,
    string TransactionId
) : DomainEventBase;

/// <summary>
/// Raised when a booking is created and accepted by the system.
/// Consumed by: notification handlers that post to the branch's operations feed.
/// </summary>
public sealed record BookingConfirmedEvent(
    string BookingId,
    string TenantId,
    string BranchId,
    string CustomerId,
    string PlateNumber,
    DateTime SlotStartUtc,
    DateTime SlotEndUtc,
    string? VehicleLabel
) : DomainEventBase;

/// <summary>
/// Raised when a cashier (or the customer via Connect) marks a booking as
/// <see cref="Enums.BookingStatus.Arrived"/> — the physical check-in moment.
/// Consumed by: notification handlers that alert the branch ops feed and refresh
/// queue-board projections.
/// </summary>
public sealed record BookingArrivedEvent(
    string BookingId,
    string TenantId,
    string BranchId,
    string CustomerId,
    string PlateNumber,
    DateTime SlotStartUtc,
    string? QueueEntryId,
    string? QueueNumber
) : DomainEventBase;

/// <summary>
/// Raised when a booking is auto-flipped to <see cref="Enums.BookingStatus.NoShow"/>
/// by the Hangfire no-show sweep.
/// </summary>
public sealed record BookingNoShowEvent(
    string BookingId,
    string TenantId,
    string BranchId,
    string CustomerId,
    string PlateNumber,
    DateTime SlotStartUtc
) : DomainEventBase;

/// <summary>
/// Raised when the 2-hour reminder SMS has been dispatched for a booking.
/// Primarily an audit signal; handlers may persist an in-app notification too.
/// </summary>
public sealed record BookingReminderSentEvent(
    string BookingId,
    string TenantId,
    string BranchId,
    string CustomerId,
    string PlateNumber,
    DateTime SlotStartUtc
) : DomainEventBase;

/// <summary>
/// Raised when the "position ahead" count for a QueueEntry may have changed
/// (e.g. a new entry joined at a higher priority, or an entry ahead was called /
/// cancelled / marked no-show). Lightweight — carries only identifiers; recipients
/// re-query the queue snapshot for the authoritative state.
/// </summary>
public sealed record QueuePositionChangedEvent(
    string QueueEntryId,
    string TenantId,
    string BranchId,
    int PositionAhead
) : DomainEventBase;
