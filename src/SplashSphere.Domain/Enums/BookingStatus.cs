namespace SplashSphere.Domain.Enums;

/// <summary>
/// Lifecycle state of an online <see cref="Entities.Booking"/>.
/// <para>
/// Transitions:
/// <c>Confirmed → Arrived → InService → Completed</c>
/// or <c>Confirmed → Cancelled</c>
/// or <c>Confirmed → NoShow</c> (after grace period).
/// </para>
/// </summary>
public enum BookingStatus
{
    /// <summary>Booking created and accepted. Default state on creation.</summary>
    Confirmed = 1,

    /// <summary>Customer has checked in — either via the Connect app or by the cashier.</summary>
    Arrived = 2,

    /// <summary>Service has begun (linked to a <see cref="Entities.Transaction"/>).</summary>
    InService = 3,

    /// <summary>Service finished. Terminal.</summary>
    Completed = 4,

    /// <summary>Cancelled by the customer or tenant. Terminal.</summary>
    Cancelled = 5,

    /// <summary>Customer did not arrive within the configured grace period. Terminal.</summary>
    NoShow = 6,
}
