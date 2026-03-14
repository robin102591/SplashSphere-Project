using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Reports.Queries.GetServicePopularityReport;

/// <param name="From">Start date (Manila, inclusive).</param>
/// <param name="To">End date (Manila, inclusive).</param>
/// <param name="BranchId">Filter to a single branch. Null = all branches.</param>
/// <param name="Top">Maximum number of services/packages to return (default 20).</param>
public sealed record GetServicePopularityReportQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId = null,
    int Top = 20) : IQuery<ServicePopularityReportDto>;
