using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Reports.Queries.GetRevenueReport;

/// <param name="From">Start date (Manila, inclusive).</param>
/// <param name="To">End date (Manila, inclusive).</param>
/// <param name="BranchId">Filter to a single branch. Null = all branches.</param>
public sealed record GetRevenueReportQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId = null) : IQuery<RevenueReportDto>;
