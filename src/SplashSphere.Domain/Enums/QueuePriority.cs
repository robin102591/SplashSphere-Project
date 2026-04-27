namespace SplashSphere.Domain.Enums;

/// <summary>
/// Priority level assigned to a queue entry at the time of check-in.
/// Higher-priority entries are called before lower-priority ones.
/// Within the same priority tier, entries are served FIFO by <c>CreatedAt</c>.
/// </summary>
public enum QueuePriority
{
    /// <summary>
    /// Standard walk-in customer. No special treatment.
    /// Lowest priority tier.
    /// </summary>
    Regular = 1,

    /// <summary>
    /// Loyalty member, fleet account, or staff-designated fast-lane customer.
    /// Served before <see cref="Regular"/> entries.
    /// </summary>
    Express = 2,

    /// <summary>
    /// Customer who pre-booked their slot via the Connect app.
    /// Auto-assigned by the <c>CreateQueueFromBookings</c> job ~15 minutes
    /// before the slot. Served before <see cref="Express"/> but below <see cref="Vip"/>.
    /// </summary>
    Booked = 3,

    /// <summary>
    /// Premium membership holder or branch-manager override.
    /// Highest priority tier — served before all other entries.
    /// </summary>
    Vip = 4,
}
