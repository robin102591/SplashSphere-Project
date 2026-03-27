using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Queries.GetEmployeePayrollHistory;

public sealed class GetEmployeePayrollHistoryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetEmployeePayrollHistoryQuery, PagedResult<EmployeePayrollHistoryDto>>
{
    public async Task<PagedResult<EmployeePayrollHistoryDto>> Handle(
        GetEmployeePayrollHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.PayrollEntries
            .AsNoTracking()
            .Where(e => e.EmployeeId == request.EmployeeId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.PayrollPeriod.StartDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new EmployeePayrollHistoryDto(
                e.Id,
                e.PayrollPeriodId,
                e.PayrollPeriod.StartDate,
                e.PayrollPeriod.EndDate,
                (int)e.PayrollPeriod.Status,
                e.DaysWorked,
                e.BaseSalary,
                e.TotalCommissions,
                e.TotalTips,
                e.Bonuses,
                e.Deductions,
                e.BaseSalary + e.TotalCommissions + e.Bonuses - e.Deductions))
            .ToListAsync(cancellationToken);

        return PagedResult<EmployeePayrollHistoryDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
