using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Expenses.Commands.DeleteExpense;

public sealed record DeleteExpenseCommand(string Id) : ICommand;
