namespace SplashSphere.Domain.Enums;

/// <summary>
/// Categorises a maintenance activity performed on equipment.
/// </summary>
public enum MaintenanceType
{
    /// <summary>Scheduled maintenance to prevent breakdowns.</summary>
    Preventive = 1,

    /// <summary>Repair performed in response to a fault or breakdown.</summary>
    Corrective = 2,

    /// <summary>Routine check or assessment without repair.</summary>
    Inspection = 3,

    /// <summary>Replacement of a worn or broken component.</summary>
    PartReplacement = 4,
}
