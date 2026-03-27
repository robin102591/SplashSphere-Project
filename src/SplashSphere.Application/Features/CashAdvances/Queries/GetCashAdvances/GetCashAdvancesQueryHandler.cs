using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.CashAdvances.Queries.GetCashAdvances;

public sealed class GetCashAdvancesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCashAdvancesQuery, PagedResult<CashAdvanceDto>>
{
    public async Task<PagedResult<CashAdvanceDto>> Handle(
        GetCashAdvancesQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.CashAdvances
            .AsNoTracking()
            .Include(ca => ca.Employee)
            .Include(ca => ca.ApprovedBy)
            .AsQueryable();

        if (request.EmployeeId is not null)
            query = query.Where(ca => ca.EmployeeId == request.EmployeeId);

        if (request.Status is not null)
            query = query.Where(ca => ca.Status == request.Status);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(ca => ca.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ca => new CashAdvanceDto(
                ca.Id,
                ca.EmployeeId,
                ca.Employee.FirstName + " " + ca.Employee.LastName,
                ca.Amount,
                ca.RemainingBalance,
                ca.Status,
                ca.Reason,
                ca.ApprovedBy != null ? ca.ApprovedBy.FirstName + " " + ca.ApprovedBy.LastName : null,
                ca.ApprovedAt,
                ca.DeductionPerPeriod,
                ca.CreatedAt,
                ca.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<CashAdvanceDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
