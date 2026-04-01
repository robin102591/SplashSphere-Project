using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Expenses.Commands.RecordExpense;

public sealed record RecordExpenseCommand(
    string BranchId,
    string CategoryId,
    decimal Amount,
    string Description,
    DateTime ExpenseDate,
    string? Vendor = null,
    string? ReceiptReference = null,
    ExpenseFrequency Frequency = ExpenseFrequency.OneTime,
    bool IsRecurring = false) : ICommand<string>;
