namespace SplashSphere.Application.Features.Reports.Queries.GetRevenueReport;

public sealed record RevenueReportDto(
    DateOnly From,
    DateOnly To,
    string? BranchId,
    string? BranchName,
    decimal GrandTotal,
    decimal TotalDiscount,
    decimal TotalTax,
    int TransactionCount,
    IReadOnlyList<RevenueDayDto> DailyBreakdown,
    IReadOnlyList<RevenueByPaymentMethodDto> ByPaymentMethod);

public sealed record RevenueDayDto(
    DateOnly Date,
    decimal Revenue,
    decimal Discount,
    decimal Tax,
    int TransactionCount);

public sealed record RevenueByPaymentMethodDto(
    string PaymentMethod,
    decimal Amount,
    int PaymentCount);
