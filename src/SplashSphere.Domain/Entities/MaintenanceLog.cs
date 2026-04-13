using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// Records a maintenance activity performed on an <see cref="Equipment"/> item.
/// Supports scheduling the next maintenance via <see cref="NextDueDate"/> or
/// <see cref="NextDueHours"/> (operating hours).
/// </summary>
public sealed class MaintenanceLog : IAuditableEntity
{
    private MaintenanceLog() { } // EF Core

    public MaintenanceLog(string equipmentId, MaintenanceType type, string description, DateTime performedDate)
    {
        Id = Guid.NewGuid().ToString();
        EquipmentId = equipmentId;
        Type = type;
        Description = description;
        PerformedDate = performedDate;
    }

    public string Id { get; set; } = string.Empty;
    public string EquipmentId { get; set; } = string.Empty;
    public MaintenanceType Type { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>Cost of this maintenance activity in PHP. Precision (10, 2).</summary>
    public decimal? Cost { get; set; }

    /// <summary>Name of the person or company that performed the maintenance.</summary>
    public string? PerformedBy { get; set; }

    public DateTime PerformedDate { get; set; }

    /// <summary>Calendar date when next maintenance is due (preventive scheduling).</summary>
    public DateTime? NextDueDate { get; set; }

    /// <summary>Operating hours at which next maintenance is due (alternative to date-based scheduling).</summary>
    public int? NextDueHours { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Equipment Equipment { get; set; } = null!;
}
