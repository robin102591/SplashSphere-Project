using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Expenses.Commands.UpdateExpense;

public sealed record UpdateExpenseCommand(
    string Id,
    string CategoryId,
    decimal Amount,
    string Description,
    DateTime ExpenseDate,
    string? Vendor,
    string? ReceiptReference,
    ExpenseFrequency Frequency,
    bool IsRecurring) : ICommand;
