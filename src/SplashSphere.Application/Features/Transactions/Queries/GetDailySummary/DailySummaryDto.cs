using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Transactions.Queries.GetDailySummary;

public sealed record DailySummaryDto(
    DateOnly Date,
    string BranchId,
    string BranchName,
    int TotalTransactions,
    int CompletedTransactions,
    int CancelledTransactions,
    int PendingTransactions,
    decimal TotalRevenue,
    decimal TotalDiscounts,
    decimal TotalTax,
    IReadOnlyList<PaymentBreakdownDto> PaymentBreakdown,
    IReadOnlyList<TopServiceDto> TopServices);

public sealed record PaymentBreakdownDto(
    PaymentMethod Method,
    int Count,
    decimal TotalAmount);

public sealed record TopServiceDto(
    string ServiceId,
    string ServiceName,
    int Count,
    decimal TotalRevenue);
