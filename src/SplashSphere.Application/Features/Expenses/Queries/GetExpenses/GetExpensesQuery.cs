using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Expenses.Queries.GetExpenses;

public sealed record GetExpensesQuery(
    string? BranchId = null,
    string? CategoryId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = 50) : IQuery<PagedResult<ExpenseDto>>;
