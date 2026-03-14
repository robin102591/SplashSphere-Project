namespace SplashSphere.Application.Features.Reports.Queries.GetCommissionsReport;

public sealed record CommissionsReportDto(
    DateOnly From,
    DateOnly To,
    string? BranchId,
    string? EmployeeId,
    decimal GrandTotalCommissions,
    int TransactionCount,
    IReadOnlyList<EmployeeCommissionDto> Employees);

public sealed record EmployeeCommissionDto(
    string EmployeeId,
    string EmployeeName,
    string BranchName,
    string EmployeeType,
    decimal TotalCommissions,
    int TransactionCount);
