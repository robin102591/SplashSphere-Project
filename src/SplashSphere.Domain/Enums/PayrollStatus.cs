namespace SplashSphere.Domain.Enums;

/// <summary>
/// Lifecycle states of a weekly payroll period.
/// Transitions are strictly sequential: Open → Closed → Processed.
/// No state may be skipped, and Processed is immutable.
/// </summary>
public enum PayrollStatus
{
    /// <summary>
    /// The period is active. Commissions and attendance are still being accumulated.
    /// Entries can be created but not yet finalised.
    /// Created automatically every Monday 00:00 PHT by <c>CreateWeeklyPayrollPeriod</c> job.
    /// </summary>
    Open = 1,

    /// <summary>
    /// The period has ended and totals have been computed per employee.
    /// Admins may review and adjust bonuses/deductions at this stage.
    /// Closed automatically on Sunday 23:55 PHT by <c>AutoClosePayrollPeriod</c> job.
    /// </summary>
    Closed = 2,

    /// <summary>
    /// Final state. Payroll has been approved and disbursed.
    /// No modifications are permitted. Audit trail is frozen.
    /// </summary>
    Processed = 3,
}
