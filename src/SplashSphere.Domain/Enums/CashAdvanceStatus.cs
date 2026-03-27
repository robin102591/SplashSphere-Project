namespace SplashSphere.Domain.Enums;

/// <summary>
/// Lifecycle status of a <see cref="Entities.CashAdvance"/>.
/// </summary>
public enum CashAdvanceStatus
{
    /// <summary>Requested by employee or admin, awaiting approval.</summary>
    Pending = 1,

    /// <summary>Manager approved, ready to disburse.</summary>
    Approved = 2,

    /// <summary>Disbursed to employee, being deducted from payroll each period.</summary>
    Active = 3,

    /// <summary>Remaining balance fully settled via payroll deductions.</summary>
    FullyPaid = 4,

    /// <summary>Rejected or withdrawn before disbursement.</summary>
    Cancelled = 5
}
