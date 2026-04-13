using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetSupplyUsageTrend;

public sealed record GetSupplyUsageTrendQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId = null) : IQuery<SupplyUsageTrendDto>;
