using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Queries.GetEmployees;

public sealed record GetEmployeesQuery(
    int Page = 1,
    int PageSize = 20,
    string? BranchId = null,
    EmployeeType? EmployeeType = null,
    string? Search = null) : IQuery<PagedResult<EmployeeDto>>;
