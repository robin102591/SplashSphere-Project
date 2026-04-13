namespace SplashSphere.Domain.Enums;

/// <summary>
/// Indicates the current operational state of a piece of equipment.
/// </summary>
public enum EquipmentStatus
{
    /// <summary>Equipment is functional and in active use.</summary>
    Operational = 1,

    /// <summary>Equipment is still usable but has been flagged for maintenance.</summary>
    NeedsMaintenance = 2,

    /// <summary>Equipment is currently out of service for repair.</summary>
    UnderRepair = 3,

    /// <summary>Equipment has been permanently decommissioned.</summary>
    Retired = 4,
}
