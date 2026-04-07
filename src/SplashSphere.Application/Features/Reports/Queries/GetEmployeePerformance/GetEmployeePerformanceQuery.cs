using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Reports.Queries.GetEmployeePerformance;

public sealed record GetEmployeePerformanceQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId = null) : IQuery<EmployeePerformanceDto>;
