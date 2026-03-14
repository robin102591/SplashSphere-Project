namespace SplashSphere.Domain.Enums;

/// <summary>
/// Lifecycle states of a POS transaction.
/// Valid transitions: Pending → InProgress → Completed.
/// Cancellation is allowed from Pending or InProgress.
/// Refund is only allowed from Completed.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Transaction has been created but service has not yet started.
    /// May be cancelled at this stage.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Service is actively being performed on the vehicle.
    /// May be cancelled at this stage.
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Service is finished and payment has been collected in full.
    /// Triggers commission calculation and inventory decrement.
    /// Immutable except for refund.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Transaction was voided before completion. No commissions are earned.
    /// Inventory is not decremented for merchandise items.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// A completed transaction that was reversed. Commissions are clawed back
    /// from the relevant payroll period if still open.
    /// </summary>
    Refunded = 5,
}
