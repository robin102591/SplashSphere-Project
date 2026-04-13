using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetEquipmentMaintenanceReport;

public sealed record GetEquipmentMaintenanceReportQuery(
    string? BranchId = null) : IQuery<EquipmentMaintenanceReportDto>;
