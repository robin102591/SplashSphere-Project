namespace SplashSphere.Domain.Entities;

/// <summary>
/// The computed payroll summary for one employee within a <see cref="PayrollPeriod"/>.
/// Created when the period transitions from Open → Closed.
/// <para>
/// Fields are snapshots captured at close time so that subsequent changes to rates
/// or commission matrices do not retroactively alter finalised payroll.
/// </para>
/// <para>
/// Adjustable fields (<see cref="Bonuses"/>, <see cref="Deductions"/>, <see cref="Notes"/>)
/// may be edited while the parent period is <see cref="PayrollStatus.Closed"/>.
/// Once the period reaches <see cref="PayrollStatus.Processed"/> no further changes are allowed.
/// </para>
/// Unique constraint: [PayrollPeriodId, EmployeeId] — one entry per employee per period.
/// </summary>
public sealed class PayrollEntry : IAuditableEntity
{
    private PayrollEntry() { } // EF Core

    public PayrollEntry(
        string tenantId,
        string payrollPeriodId,
        string employeeId,
        EmployeeType employeeTypeSnapshot,
        int daysWorked,
        decimal? dailyRateSnapshot,
        decimal baseSalary,
        decimal totalCommissions)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        PayrollPeriodId = payrollPeriodId;
        EmployeeId = employeeId;
        EmployeeTypeSnapshot = employeeTypeSnapshot;
        DaysWorked = daysWorked;
        DailyRateSnapshot = dailyRateSnapshot;
        BaseSalary = baseSalary;
        TotalCommissions = totalCommissions;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PayrollPeriodId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;

    /// <summary>
    /// Snapshot of the employee's <see cref="EmployeeType"/> at close time.
    /// Guards against type changes affecting historical payroll.
    /// </summary>
    public EmployeeType EmployeeTypeSnapshot { get; set; }

    /// <summary>
    /// Number of attendance records within the period.
    /// Meaningful for <see cref="EmployeeType.Daily"/>; 0 for commission-only employees.
    /// </summary>
    public int DaysWorked { get; set; }

    /// <summary>
    /// Snapshot of the employee's daily rate at close time. Precision (10, 2).
    /// Null for <see cref="EmployeeType.Commission"/> employees.
    /// </summary>
    public decimal? DailyRateSnapshot { get; set; }

    /// <summary>
    /// For <see cref="EmployeeType.Daily"/>: <c>DailyRateSnapshot × DaysWorked</c>.
    /// For <see cref="EmployeeType.Commission"/>: always 0. Precision (10, 2).
    /// </summary>
    public decimal BaseSalary { get; set; }

    /// <summary>
    /// Sum of all commission amounts earned from completed transactions in this period.
    /// Split amounts are already stored at the individual transaction level;
    /// this is the rolled-up total. Precision (10, 2).
    /// </summary>
    public decimal TotalCommissions { get; set; }

    /// <summary>
    /// Admin-adjustable bonus amount. Editable while period is Closed. Precision (10, 2).
    /// </summary>
    public decimal Bonuses { get; set; }

    /// <summary>
    /// Admin-adjustable deduction amount (e.g. cash advance, SSS). Editable while period is Closed.
    /// Precision (10, 2).
    /// </summary>
    public decimal Deductions { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Computed ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Final take-home amount: <c>BaseSalary + TotalCommissions + Bonuses − Deductions</c>.
    /// Calculated in memory from stored components; not persisted as a column.
    /// </summary>
    public decimal NetPay => BaseSalary + TotalCommissions + Bonuses - Deductions;

    // ── Navigations ──────────────────────────────────────────────────────────

    public PayrollPeriod PayrollPeriod { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
