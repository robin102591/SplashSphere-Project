using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Expenses.Queries.GetExpenses;

public sealed class GetExpensesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetExpensesQuery, PagedResult<ExpenseDto>>
{
    public async Task<PagedResult<ExpenseDto>> Handle(
        GetExpensesQuery request, CancellationToken cancellationToken)
    {
        var query = db.Expenses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            query = query.Where(e => e.BranchId == request.BranchId);

        if (!string.IsNullOrWhiteSpace(request.CategoryId))
            query = query.Where(e => e.CategoryId == request.CategoryId);

        if (request.From.HasValue)
        {
            var fromUtc = request.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(e => e.ExpenseDate >= fromUtc);
        }

        if (request.To.HasValue)
        {
            var toUtc = request.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(e => e.ExpenseDate < toUtc);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new ExpenseDto(
                e.Id,
                e.Branch.Name,
                e.Category.Name,
                e.Category.Icon,
                e.Amount,
                e.Description,
                e.Vendor,
                e.ReceiptReference,
                e.ExpenseDate,
                e.Frequency,
                e.IsRecurring,
                e.RecordedBy.FirstName + " " + e.RecordedBy.LastName,
                e.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<ExpenseDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
