using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.AttendanceReports.Queries.GetAttendanceReport;

public sealed record GetAttendanceReportQuery(
    DateOnly From,
    DateOnly To,
    string? BranchId = null,
    string? EmployeeId = null,
    int ExpectedWorkDaysPerWeek = 6) : IQuery<AttendanceReportDto>;
