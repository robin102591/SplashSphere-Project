using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Expenses;

public sealed record ExpenseDto(
    string Id,
    string BranchName,
    string CategoryName,
    string? CategoryIcon,
    decimal Amount,
    string Description,
    string? Vendor,
    string? ReceiptReference,
    DateTime ExpenseDate,
    ExpenseFrequency Frequency,
    bool IsRecurring,
    string RecordedByName,
    DateTime CreatedAt);

public sealed record ExpenseCategoryDto(
    string Id,
    string Name,
    string? Icon,
    bool IsActive);

public sealed record ProfitLossReportDto(
    DateOnly From,
    DateOnly To,
    string? BranchId,
    string? BranchName,
    decimal Revenue,
    decimal Cogs,
    decimal GrossProfit,
    decimal TotalExpenses,
    decimal NetProfit,
    decimal MarginPercent,
    IReadOnlyList<ExpenseByCategoryDto> ExpensesByCategory,
    IReadOnlyList<ProfitLossDayDto> DailyBreakdown);

public sealed record ExpenseByCategoryDto(
    string CategoryName,
    decimal Amount);

public sealed record ProfitLossDayDto(
    DateOnly Date,
    decimal Revenue,
    decimal Expenses,
    decimal NetProfit);
