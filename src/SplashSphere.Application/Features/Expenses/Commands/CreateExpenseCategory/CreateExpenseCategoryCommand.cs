using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Expenses.Commands.CreateExpenseCategory;

public sealed record CreateExpenseCategoryCommand(string Name, string? Icon = null) : ICommand<string>;
