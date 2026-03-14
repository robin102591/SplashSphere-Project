using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Dashboard.Queries.GetDashboardSummary;

/// <summary>
/// When <paramref name="BranchId"/> is provided, KPIs are scoped to that branch
/// and <see cref="DashboardSummaryDto.Branches"/> is null.
/// When omitted, returns tenant-wide totals plus per-branch breakdowns.
/// </summary>
public sealed record GetDashboardSummaryQuery(string? BranchId = null)
    : IQuery<DashboardSummaryDto>;
