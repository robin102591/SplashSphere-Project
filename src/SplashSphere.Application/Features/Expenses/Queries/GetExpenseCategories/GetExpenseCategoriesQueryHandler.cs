using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Expenses.Queries.GetExpenseCategories;

public sealed class GetExpenseCategoriesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetExpenseCategoriesQuery, IReadOnlyList<ExpenseCategoryDto>>
{
    public async Task<IReadOnlyList<ExpenseCategoryDto>> Handle(
        GetExpenseCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await db.ExpenseCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new ExpenseCategoryDto(c.Id, c.Name, c.Icon, c.IsActive))
            .ToListAsync(cancellationToken);
    }
}
