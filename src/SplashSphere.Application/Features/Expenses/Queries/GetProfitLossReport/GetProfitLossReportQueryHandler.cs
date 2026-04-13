using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Expenses.Queries.GetProfitLossReport;

public sealed class GetProfitLossReportQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProfitLossReportQuery, ProfitLossReportDto>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<ProfitLossReportDto> Handle(
        GetProfitLossReportQuery request,
        CancellationToken cancellationToken)
    {
        var fromUtc = DateTime.SpecifyKind(request.From.ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(request.To.AddDays(1).ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);

        // ── Revenue from completed transactions ─────────────────────────────
        var txQuery = db.Transactions
            .AsNoTracking()
            .Where(t => t.Status == TransactionStatus.Completed &&
                        t.CompletedAt >= fromUtc && t.CompletedAt < toUtc);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            txQuery = txQuery.Where(t => t.BranchId == request.BranchId);

        var revenue = await txQuery.SumAsync(t => (decimal?)t.FinalAmount ?? 0, cancellationToken);

        // Daily revenue breakdown
        var dailyRevenue = await txQuery
            .GroupBy(t => t.CompletedAt!.Value.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(t => t.FinalAmount) })
            .ToListAsync(cancellationToken);

        // ── Expenses ────────────────────────────────────────────────────────
        var expQuery = db.Expenses.AsNoTracking()
            .Where(e => e.ExpenseDate >= fromUtc && e.ExpenseDate < toUtc);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            expQuery = expQuery.Where(e => e.BranchId == request.BranchId);

        // Load expense rows with category name (flat projection avoids GroupBy translation issues)
        var expenseRows = await expQuery
            .Select(e => new { e.Amount, CategoryName = e.Category.Name, e.ExpenseDate })
            .ToListAsync(cancellationToken);

        var totalExpenses = expenseRows.Sum(e => e.Amount);

        var expensesByCategory = expenseRows
            .GroupBy(e => e.CategoryName)
            .Select(g => new ExpenseByCategoryDto(g.Key, g.Sum(e => e.Amount)))
            .OrderByDescending(x => x.Amount)
            .ToList();

        var dailyExpenses = expenseRows
            .GroupBy(e => e.ExpenseDate.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(e => e.Amount) })
            .ToList();

        // ── COGS (merchandise cost from completed transactions) ─────────────
        // SUM(TransactionMerchandise.Quantity × Merchandise.CostPrice) for completed transactions
        var cogsQuery = db.TransactionMerchandise
            .AsNoTracking()
            .Where(tm => tm.Transaction.Status == TransactionStatus.Completed &&
                         tm.Transaction.CompletedAt >= fromUtc &&
                         tm.Transaction.CompletedAt < toUtc &&
                         tm.Merchandise.CostPrice != null);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            cogsQuery = cogsQuery.Where(tm => tm.Transaction.BranchId == request.BranchId);

        var merchandiseCogs = await cogsQuery
            .SumAsync(tm => (decimal?)(tm.Quantity * tm.Merchandise.CostPrice!.Value) ?? 0, cancellationToken);

        // ── COGS from stock movements (supply usage + merchandise sales) ────
        var smSaleQuery = db.StockMovements
            .AsNoTracking()
            .Where(sm => sm.Type == MovementType.SaleOut
                         && sm.TotalCost.HasValue
                         && sm.MovementDate >= fromUtc
                         && sm.MovementDate < toUtc);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            smSaleQuery = smSaleQuery.Where(sm => sm.BranchId == request.BranchId);

        var saleCogs = await smSaleQuery
            .SumAsync(sm => (decimal?)sm.TotalCost!.Value ?? 0, cancellationToken);

        var smUsageQuery = db.StockMovements
            .AsNoTracking()
            .Where(sm => sm.Type == MovementType.UsageOut
                         && sm.TotalCost.HasValue
                         && sm.MovementDate >= fromUtc
                         && sm.MovementDate < toUtc);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            smUsageQuery = smUsageQuery.Where(sm => sm.BranchId == request.BranchId);

        var supplyCogs = await smUsageQuery
            .SumAsync(sm => (decimal?)sm.TotalCost!.Value ?? 0, cancellationToken);

        var cogs = merchandiseCogs + saleCogs + supplyCogs;

        // ── Build daily breakdown ───────────────────────────────────────────
        var allDates = new HashSet<DateOnly>();
        foreach (var r in dailyRevenue) allDates.Add(DateOnly.FromDateTime(r.Date.Add(ManilaOffset)));
        foreach (var e in dailyExpenses) allDates.Add(DateOnly.FromDateTime(e.Date.Add(ManilaOffset)));

        var dailyBreakdown = allDates
            .Where(d => d >= request.From && d <= request.To)
            .OrderBy(d => d)
            .Select(d =>
            {
                var dayRevenue = dailyRevenue
                    .Where(r => DateOnly.FromDateTime(r.Date.Add(ManilaOffset)) == d)
                    .Sum(r => r.Amount);
                var dayExpense = dailyExpenses
                    .Where(e => DateOnly.FromDateTime(e.Date.Add(ManilaOffset)) == d)
                    .Sum(e => e.Amount);
                return new ProfitLossDayDto(d, dayRevenue, dayExpense, dayRevenue - dayExpense);
            })
            .ToList();

        // ── Compute totals ──────────────────────────────────────────────────
        var grossProfit = revenue - cogs;
        var netProfit = grossProfit - totalExpenses;
        var marginPercent = revenue > 0
            ? Math.Round(netProfit / revenue * 100, 1, MidpointRounding.AwayFromZero)
            : 0;

        // Branch name
        string? branchName = null;
        if (!string.IsNullOrWhiteSpace(request.BranchId))
        {
            branchName = await db.Branches.AsNoTracking()
                .Where(b => b.Id == request.BranchId)
                .Select(b => b.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new ProfitLossReportDto(
            request.From, request.To, request.BranchId, branchName,
            revenue, cogs, grossProfit, totalExpenses,
            netProfit, marginPercent,
            expensesByCategory, dailyBreakdown);
    }
}
