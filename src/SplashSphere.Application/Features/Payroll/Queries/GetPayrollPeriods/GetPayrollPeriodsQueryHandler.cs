using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Queries.GetPayrollPeriods;

public sealed class GetPayrollPeriodsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPayrollPeriodsQuery, PagedResult<PayrollPeriodSummaryDto>>
{
    public async Task<PagedResult<PayrollPeriodSummaryDto>> Handle(
        GetPayrollPeriodsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.PayrollPeriods.AsNoTracking();

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.Year.HasValue)
            query = query.Where(p => p.Year == request.Year.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.CutOffWeek)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PayrollPeriodSummaryDto(
                p.Id,
                p.Status,
                p.Year,
                p.CutOffWeek,
                p.StartDate,
                p.EndDate,
                p.Entries.Count,
                p.Entries.Sum(e => e.BaseSalary + e.TotalCommissions + e.Bonuses - e.Deductions),
                p.ScheduledReleaseDate,
                p.ReleasedAt,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<PayrollPeriodSummaryDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
