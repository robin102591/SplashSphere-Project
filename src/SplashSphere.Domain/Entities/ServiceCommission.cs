namespace SplashSphere.Domain.Entities;

/// <summary>
/// One cell in the service commission matrix: the commission rule for a specific
/// (service, vehicleType, size) combination.
/// <para>
/// Calculation rules by <see cref="Type"/>:
/// <list type="bullet">
///   <item><see cref="CommissionType.Percentage"/> — <c>price × PercentageRate / 100</c>. Only <see cref="PercentageRate"/> is set.</item>
///   <item><see cref="CommissionType.FixedAmount"/> — <see cref="FixedAmount"/> PHP regardless of price. Only <see cref="FixedAmount"/> is set.</item>
///   <item><see cref="CommissionType.Hybrid"/> — <c>FixedAmount + (price × PercentageRate / 100)</c>. Both fields are set.</item>
/// </list>
/// No matrix entry for a combination means ₱0 commission for that cell.
/// </para>
/// Unique constraint: (serviceId, vehicleTypeId, sizeId).
/// </summary>
public sealed class ServiceCommission : IAuditableEntity
{
    private ServiceCommission() { } // EF Core

    public ServiceCommission(
        string tenantId,
        string serviceId,
        string vehicleTypeId,
        string sizeId,
        CommissionType type,
        decimal? fixedAmount,
        decimal? percentageRate)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        ServiceId = serviceId;
        VehicleTypeId = vehicleTypeId;
        SizeId = sizeId;
        Type = type;
        FixedAmount = fixedAmount;
        PercentageRate = percentageRate;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string VehicleTypeId { get; set; } = string.Empty;
    public string SizeId { get; set; } = string.Empty;

    public CommissionType Type { get; set; }

    /// <summary>
    /// Fixed PHP amount component. Required for <see cref="CommissionType.FixedAmount"/>
    /// and <see cref="CommissionType.Hybrid"/>. Null for <see cref="CommissionType.Percentage"/>.
    /// Precision (10, 2).
    /// </summary>
    public decimal? FixedAmount { get; set; }

    /// <summary>
    /// Percentage rate (e.g. 15.00 means 15%). Required for <see cref="CommissionType.Percentage"/>
    /// and <see cref="CommissionType.Hybrid"/>. Null for <see cref="CommissionType.FixedAmount"/>.
    /// Precision (5, 2).
    /// </summary>
    public decimal? PercentageRate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Service Service { get; set; } = null!;
    public VehicleType VehicleType { get; set; } = null!;
    public Size Size { get; set; } = null!;
}
