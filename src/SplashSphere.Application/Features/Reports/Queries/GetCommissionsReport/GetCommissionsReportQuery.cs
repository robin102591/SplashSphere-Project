using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Reports.Queries.GetCommissionsReport;

/// <param name="From">Start date (Manila, inclusive).</param>
/// <param name="To">End date (Manila, inclusive).</param>
/// <param name="BranchId">Filter to employees of a specific branch.</param>
/// <param name="EmployeeId">Filter to a single employee.</param>
public sealed record GetCommissionsReportQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId    = null,
    string? EmployeeId  = null) : IQuery<CommissionsReportDto>;
