using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Queries.GetEmployeePayrollHistory;

public sealed record GetEmployeePayrollHistoryQuery(
    string EmployeeId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<EmployeePayrollHistoryDto>>;

public sealed record EmployeePayrollHistoryDto(
    string EntryId,
    string PeriodId,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    int PeriodStatus,
    int DaysWorked,
    decimal BaseSalary,
    decimal TotalCommissions,
    decimal TotalTips,
    decimal Bonuses,
    decimal Deductions,
    decimal NetPay);
