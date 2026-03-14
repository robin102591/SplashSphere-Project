using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Queries.GetAttendance;

/// <summary>
/// Paginated attendance records, filterable by branch and/or date range.
/// Designed for the admin attendance view — shows all employees at a branch
/// for a given period, or all records for a specific employee.
/// </summary>
public sealed record GetAttendanceQuery(
    int Page = 1,
    int PageSize = 50,
    string? BranchId = null,
    string? EmployeeId = null,
    DateOnly? From = null,
    DateOnly? To = null) : IQuery<PagedResult<AttendanceDto>>;

public sealed record AttendanceDto(
    string Id,
    string EmployeeId,
    string EmployeeFullName,
    string BranchName,
    DateOnly Date,
    DateTime TimeIn,
    DateTime? TimeOut,
    string? Notes);
