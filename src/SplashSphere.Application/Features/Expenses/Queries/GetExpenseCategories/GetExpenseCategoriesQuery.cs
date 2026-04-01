using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Expenses.Queries.GetExpenseCategories;

public sealed record GetExpenseCategoriesQuery : IQuery<IReadOnlyList<ExpenseCategoryDto>>;
