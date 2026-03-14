using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Employees.Queries.GetEmployeeCommissions;

public sealed class GetEmployeeCommissionsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetEmployeeCommissionsQuery, PagedResult<EmployeeCommissionDto>>
{
    public async Task<PagedResult<EmployeeCommissionDto>> Handle(
        GetEmployeeCommissionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.TransactionEmployees
            .AsNoTracking()
            .Where(te => te.EmployeeId == request.EmployeeId);

        if (request.From.HasValue)
        {
            var fromUtc = request.From.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(te => te.Transaction.CreatedAt >= fromUtc);
        }

        if (request.To.HasValue)
        {
            // Include the full To day.
            var toUtc = request.To.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(te => te.Transaction.CreatedAt <= toUtc);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(te => te.Transaction.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(te => new EmployeeCommissionDto(
                te.TransactionId,
                te.Transaction.TransactionNumber,
                DateOnly.FromDateTime(te.Transaction.CreatedAt),
                te.Transaction.Branch.Name,
                te.TotalCommission))
            .ToListAsync(cancellationToken);

        return PagedResult<EmployeeCommissionDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
