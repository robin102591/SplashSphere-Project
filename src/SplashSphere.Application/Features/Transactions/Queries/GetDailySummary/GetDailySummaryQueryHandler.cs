using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Transactions.Queries.GetDailySummary;

public sealed class GetDailySummaryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDailySummaryQuery, DailySummaryDto>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<DailySummaryDto> Handle(
        GetDailySummaryQuery request,
        CancellationToken cancellationToken)
    {
        var targetDate   = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow + ManilaOffset);
        var fromUtc      = targetDate.ToDateTime(TimeOnly.MinValue) - ManilaOffset;
        var toUtc        = fromUtc.AddDays(1);

        // ── Query 1: branch name ──────────────────────────────────────────────
        var branchName = await context.Branches
            .AsNoTracking()
            .Where(b => b.Id == request.BranchId)
            .Select(b => b.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        // ── Query 2: transaction summary scalars ──────────────────────────────
        var transactions = await context.Transactions
            .AsNoTracking()
            .Where(t => t.BranchId == request.BranchId
                     && t.CreatedAt >= fromUtc
                     && t.CreatedAt < toUtc)
            .Select(t => new { t.Status, t.TotalAmount, t.DiscountAmount, t.TaxAmount, t.FinalAmount })
            .ToListAsync(cancellationToken);

        var completedTxs     = transactions.Where(t => t.Status == TransactionStatus.Completed).ToList();
        var totalTransactions = transactions.Count;
        var completed         = completedTxs.Count;
        var cancelled         = transactions.Count(t => t.Status == TransactionStatus.Cancelled);
        var pending           = transactions.Count(t =>
            t.Status is TransactionStatus.Pending or TransactionStatus.InProgress);

        var totalRevenue   = completedTxs.Sum(t => t.FinalAmount);
        var totalDiscounts = completedTxs.Sum(t => t.DiscountAmount);
        var totalTax       = completedTxs.Sum(t => t.TaxAmount);

        // ── Query 3: payment method breakdown (completed txs only) ────────────
        // Load all transaction IDs that are Completed so we can join to payments
        var completedTransactionIds = await context.Transactions
            .AsNoTracking()
            .Where(t => t.BranchId == request.BranchId
                     && t.CreatedAt >= fromUtc
                     && t.CreatedAt < toUtc
                     && t.Status == TransactionStatus.Completed)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var paymentBreakdown = new List<PaymentBreakdownDto>();

        if (completedTransactionIds.Count > 0)
        {
            var rawPayments = await context.Payments
                .AsNoTracking()
                .Where(p => completedTransactionIds.Contains(p.TransactionId))
                .Select(p => new { p.PaymentMethod, p.Amount })
                .ToListAsync(cancellationToken);

            paymentBreakdown = rawPayments
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentBreakdownDto(g.Key, g.Count(), g.Sum(p => p.Amount)))
                .OrderByDescending(p => p.TotalAmount)
                .ToList();
        }

        // ── Query 4: top services (completed txs only) ────────────────────────
        var topServices = new List<TopServiceDto>();

        if (completedTransactionIds.Count > 0)
        {
            var rawServiceLines = await context.TransactionServices
                .AsNoTracking()
                .Where(ts => completedTransactionIds.Contains(ts.TransactionId))
                .Select(ts => new { ts.ServiceId, ts.Service.Name, ts.UnitPrice })
                .ToListAsync(cancellationToken);

            topServices = rawServiceLines
                .GroupBy(s => new { s.ServiceId, s.Name })
                .Select(g => new TopServiceDto(
                    g.Key.ServiceId,
                    g.Key.Name,
                    g.Count(),
                    g.Sum(s => s.UnitPrice)))
                .OrderByDescending(s => s.Count)
                .Take(10)
                .ToList();
        }

        return new DailySummaryDto(
            targetDate,
            request.BranchId,
            branchName,
            totalTransactions,
            completed,
            cancelled,
            pending,
            totalRevenue,
            totalDiscounts,
            totalTax,
            paymentBreakdown,
            topServices);
    }
}
