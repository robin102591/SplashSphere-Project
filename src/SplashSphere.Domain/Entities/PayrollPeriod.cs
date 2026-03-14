namespace SplashSphere.Domain.Entities;

/// <summary>
/// A weekly payroll period for a single tenant. Created automatically every
/// Monday 00:00 PHT by the <c>CreateWeeklyPayrollPeriod</c> Hangfire job.
/// <para>
/// State machine — transitions are strictly sequential, no skipping:
/// <code>Open → Closed → Processed</code>
/// </para>
/// <list type="bullet">
///   <item><b>Open</b> — commissions and attendance are accumulating. No entries finalised.</item>
///   <item><b>Closed</b> — totals computed, <see cref="PayrollEntry"/> rows created.
///   Admin may adjust <c>Bonuses</c>/<c>Deductions</c> on individual entries.</item>
///   <item><b>Processed</b> — approved and disbursed. Fully immutable.</item>
/// </list>
/// Unique constraint: [TenantId, Year, CutOffWeek] — one period per ISO week per tenant.
/// </summary>
public sealed class PayrollPeriod : IAuditableEntity
{
    private PayrollPeriod() { } // EF Core

    public PayrollPeriod(string tenantId, int year, int cutOffWeek, DateOnly startDate, DateOnly endDate)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        Year = year;
        CutOffWeek = cutOffWeek;
        StartDate = startDate;
        EndDate = endDate;
        Status = PayrollStatus.Open;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public PayrollStatus Status { get; set; }

    /// <summary>ISO calendar year (e.g. 2025).</summary>
    public int Year { get; set; }

    /// <summary>ISO week number within the year (1–53).</summary>
    public int CutOffWeek { get; set; }

    /// <summary>Monday of the payroll week (Asia/Manila date), stored as SQL <c>date</c>.</summary>
    public DateOnly StartDate { get; set; }

    /// <summary>Sunday of the payroll week (Asia/Manila date), stored as SQL <c>date</c>.</summary>
    public DateOnly EndDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public ICollection<PayrollEntry> Entries { get; set; } = [];
}
