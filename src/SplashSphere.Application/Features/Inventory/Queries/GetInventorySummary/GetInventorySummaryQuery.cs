using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetInventorySummary;

public sealed record GetInventorySummaryQuery(
    string? BranchId = null) : IQuery<InventorySummaryDto>;
