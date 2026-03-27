namespace SplashSphere.Domain.Entities;

/// <summary>
/// Defines a bracket for computing government-mandated deductions (SSS, PhilHealth, Pag-IBIG, Tax).
/// Brackets are seeded globally (not tenant-scoped) and used during payroll close when auto-calc is enabled.
/// </summary>
public sealed class GovernmentContributionBracket
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Which deduction this bracket applies to: "SSS", "PhilHealth", "PagIBIG", "Tax".</summary>
    public string DeductionType { get; set; } = string.Empty;

    /// <summary>Minimum monthly salary for this bracket (inclusive).</summary>
    public decimal MinSalary { get; set; }

    /// <summary>Maximum monthly salary for this bracket (inclusive). Null = no upper limit.</summary>
    public decimal? MaxSalary { get; set; }

    /// <summary>
    /// Fixed employee contribution for this bracket.
    /// For rate-based types like PhilHealth, store 0 and use <see cref="Rate"/> instead.
    /// </summary>
    public decimal EmployeeShare { get; set; }

    /// <summary>
    /// Rate applied to salary (0.0 to 1.0). Used for PhilHealth and Tax.
    /// For fixed-amount types (SSS, Pag-IBIG), store 0.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>Calendar year these brackets are effective for.</summary>
    public int EffectiveYear { get; set; }

    /// <summary>Sort order for bracket lookup (ascending by MinSalary).</summary>
    public int SortOrder { get; set; }
}
