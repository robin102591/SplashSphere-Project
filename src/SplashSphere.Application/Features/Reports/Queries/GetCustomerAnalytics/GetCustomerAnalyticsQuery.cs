using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Reports.Queries.GetCustomerAnalytics;

public sealed record GetCustomerAnalyticsQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId = null) : IQuery<CustomerAnalyticsDto>;
