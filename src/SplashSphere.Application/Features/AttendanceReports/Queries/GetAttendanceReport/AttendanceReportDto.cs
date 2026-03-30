namespace SplashSphere.Application.Features.AttendanceReports.Queries.GetAttendanceReport;

public sealed record AttendanceReportDto(
    DateOnly From,
    DateOnly To,
    string? BranchId,
    string? EmployeeId,
    AttendanceReportSummary Summary,
    IReadOnlyList<EmployeeAttendanceRow> Employees);

public sealed record AttendanceReportSummary(
    int TotalEmployees,
    decimal AverageAttendanceRate,
    int TotalLateArrivals,
    decimal AverageHoursPerDay);

public sealed record EmployeeAttendanceRow(
    string EmployeeId,
    string EmployeeName,
    string BranchName,
    string EmployeeType,
    int DaysPresent,
    int DaysAbsent,
    int LateCount,
    int EarlyOutCount,
    decimal TotalHours,
    decimal AverageHoursPerDay);
