namespace SplashSphere.Application.Features.Reports.Queries.GetEmployeePerformance;

public sealed record EmployeePerformanceDto(
    DateOnly From,
    DateOnly To,
    string? BranchId,
    int TotalEmployees,
    decimal TotalCommissions,
    int TotalServicesPerformed,
    IReadOnlyList<EmployeeRankingDto> Rankings);

public sealed record EmployeeRankingDto(
    string EmployeeId,
    string EmployeeName,
    string BranchName,
    string EmployeeType,
    int ServicesPerformed,
    decimal RevenueGenerated,
    decimal CommissionsEarned,
    int DaysWorked,
    int DaysLate,
    decimal AverageRevenuePerService,
    decimal AttendanceRate);
