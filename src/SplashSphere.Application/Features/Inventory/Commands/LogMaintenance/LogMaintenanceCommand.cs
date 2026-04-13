using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Inventory.Commands.LogMaintenance;

public sealed record LogMaintenanceCommand(
    string EquipmentId,
    MaintenanceType Type,
    string Description,
    decimal? Cost,
    string? PerformedBy,
    DateTime PerformedDate,
    DateTime? NextDueDate,
    int? NextDueHours,
    string? Notes) : ICommand<string>;
