namespace SplashSphere.Domain.Entities;

/// <summary>
/// Records a single work day for an employee. There is at most one record per employee
/// per calendar date (unique constraint on [EmployeeId, Date]).
/// <para>
/// Used by payroll close to count <c>daysWorked</c> for <see cref="EmployeeType.Daily"/>
/// employees: every attendance row within the payroll period where
/// <see cref="TimeIn"/> is set counts as one day worked.
/// </para>
/// <see cref="TimeOut"/> is null while the employee is still clocked in;
/// the application enforces <c>TimeIn &lt; TimeOut</c> on clock-out.
/// Both timestamps are stored as UTC and converted to Asia/Manila for display.
/// </summary>
public sealed class Attendance : IAuditableEntity
{
    private Attendance() { } // EF Core

    public Attendance(string tenantId, string employeeId, DateOnly date, DateTime timeIn)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        EmployeeId = employeeId;
        Date = date;
        TimeIn = timeIn;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;

    /// <summary>
    /// The local (Asia/Manila) calendar date of the work day.
    /// Stored as a SQL <c>date</c> column (no time component).
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>Clock-in timestamp stored as UTC.</summary>
    public DateTime TimeIn { get; set; }

    /// <summary>
    /// Clock-out timestamp stored as UTC. Null while the employee is still on shift.
    /// Must be greater than <see cref="TimeIn"/>.
    /// </summary>
    public DateTime? TimeOut { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Employee Employee { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
