namespace SplashSphere.Application.Features.Reports.Queries.GetCustomerAnalytics;

public sealed record CustomerAnalyticsDto(
    DateOnly From,
    DateOnly To,
    string? BranchId,
    int TotalCustomers,
    int NewCustomers,
    int ReturningCustomers,
    decimal RetentionRate,
    decimal AverageVisitsPerCustomer,
    decimal AverageSpendPerVisit,
    IReadOnlyList<TopCustomerDto> TopCustomers,
    IReadOnlyList<VisitFrequencyBucketDto> VisitFrequencyDistribution,
    IReadOnlyList<CustomerTrendDayDto> DailyTrend);

public sealed record TopCustomerDto(
    string CustomerId,
    string CustomerName,
    string? PlateNumber,
    int VisitCount,
    decimal TotalSpent,
    decimal AverageSpend,
    DateOnly LastVisit);

public sealed record VisitFrequencyBucketDto(
    string Bucket,
    int CustomerCount);

public sealed record CustomerTrendDayDto(
    DateOnly Date,
    int NewCustomers,
    int ReturningCustomers,
    int TotalTransactions);
