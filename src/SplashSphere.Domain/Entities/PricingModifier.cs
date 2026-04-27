namespace SplashSphere.Domain.Entities;

/// <summary>
/// A conditional price adjustment applied on top of the base <see cref="ServicePricing"/>
/// matrix. Multiple active modifiers are stacked multiplicatively (or subtracted for
/// <see cref="ModifierType.Promotion"/>).
/// <para>
/// How <see cref="Value"/> is interpreted per <see cref="Type"/>:
/// <list type="bullet">
///   <item><see cref="ModifierType.PeakHour"/> — multiplier (e.g. 1.20 = +20%). Active between <see cref="StartTime"/> and <see cref="EndTime"/>.</item>
///   <item><see cref="ModifierType.DayOfWeek"/> — multiplier (e.g. 0.90 = −10%). Active on <see cref="ActiveDayOfWeek"/>.</item>
///   <item><see cref="ModifierType.Holiday"/> — multiplier. Active on <see cref="HolidayDate"/> or matched by <see cref="HolidayName"/>.</item>
///   <item><see cref="ModifierType.Promotion"/> — absolute PHP deduction subtracted from final price. Active between <see cref="StartDate"/> and <see cref="EndDate"/>.</item>
///   <item><see cref="ModifierType.Weather"/> — multiplier. Activated manually by staff via <see cref="IsActive"/>.</item>
/// </list>
/// </para>
/// Scoped per tenant; optionally restricted to a single branch via <see cref="BranchId"/>.
/// </summary>
public sealed class PricingModifier : IAuditableEntity, ITenantScoped
{
    private PricingModifier() { } // EF Core

    public PricingModifier(string tenantId, string name, ModifierType type, decimal value)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        Name = name;
        Type = type;
        Value = value;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// When set, this modifier applies only to the specified branch.
    /// Null means it applies to all branches in the tenant.
    /// </summary>
    public string? BranchId { get; set; }

    public string Name { get; set; } = string.Empty;
    public ModifierType Type { get; set; }

    /// <summary>Multiplier or absolute peso amount — interpretation depends on <see cref="Type"/>.</summary>
    public decimal Value { get; set; }

    // ── Activation condition fields (populated based on Type) ────────────────

    /// <summary>Start of active window for <see cref="ModifierType.PeakHour"/>. Stored as UTC time component.</summary>
    public TimeOnly? StartTime { get; set; }

    /// <summary>End of active window for <see cref="ModifierType.PeakHour"/>.</summary>
    public TimeOnly? EndTime { get; set; }

    /// <summary>Target day for <see cref="ModifierType.DayOfWeek"/>.</summary>
    public DayOfWeek? ActiveDayOfWeek { get; set; }

    /// <summary>Specific calendar date for <see cref="ModifierType.Holiday"/> (one-time holidays).</summary>
    public DateOnly? HolidayDate { get; set; }

    /// <summary>Named recurring holiday for <see cref="ModifierType.Holiday"/> (e.g. "Christmas Day").</summary>
    public string? HolidayName { get; set; }

    /// <summary>Promotion/discount start date for <see cref="ModifierType.Promotion"/>.</summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>Promotion/discount end date (inclusive) for <see cref="ModifierType.Promotion"/>.</summary>
    public DateOnly? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Branch? Branch { get; set; }
}
