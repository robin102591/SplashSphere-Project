namespace SplashSphere.Application.Features.Reports.Queries.GetServicePopularityReport;

public sealed record ServicePopularityReportDto(
    DateOnly From,
    DateOnly To,
    string? BranchId,
    string? BranchName,
    IReadOnlyList<ServicePopularityItemDto> Services,
    IReadOnlyList<PackagePopularityItemDto> Packages);

public sealed record ServicePopularityItemDto(
    string ServiceId,
    string ServiceName,
    string? CategoryName,
    int TimesPerformed,
    decimal TotalRevenue,
    decimal AverageRevenue);

public sealed record PackagePopularityItemDto(
    string PackageId,
    string PackageName,
    int TimesPerformed,
    decimal TotalRevenue,
    decimal AverageRevenue);
