using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Transactions.Queries.GetDailySummary;

/// <summary>
/// Returns an aggregated daily summary for a branch on the given date (defaults to today, Asia/Manila).
/// Counts only <see cref="Domain.Enums.TransactionStatus.Completed"/> transactions for revenue figures.
/// </summary>
public sealed record GetDailySummaryQuery(
    string BranchId,
    DateOnly? Date = null) : IQuery<DailySummaryDto>;
