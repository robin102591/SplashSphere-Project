using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Reports.Queries.GetRevenueReport;

public sealed class GetRevenueReportQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetRevenueReportQuery, RevenueReportDto>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<RevenueReportDto> Handle(
        GetRevenueReportQuery request,
        CancellationToken cancellationToken)
    {
        // Convert Manila calendar range to UTC window.
        var fromUtc = DateTime.SpecifyKind(request.From.ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);
        var toUtc   = DateTime.SpecifyKind(request.To.AddDays(1).ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);

        // ── Branch name ───────────────────────────────────────────────────────
        string? branchName = null;
        if (request.BranchId is not null)
        {
            branchName = await context.Branches
                .AsNoTracking()
                .Where(b => b.Id == request.BranchId)
                .Select(b => b.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // ── Completed transactions in range ───────────────────────────────────
        var txQuery = context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == TransactionStatus.Completed &&
                t.CompletedAt >= fromUtc &&
                t.CompletedAt < toUtc);

        if (request.BranchId is not null)
            txQuery = txQuery.Where(t => t.BranchId == request.BranchId);

        // ── Daily breakdown — grouped in DB ───────────────────────────────────
        // CompletedAt is UTC; shift back to Manila for calendar grouping.
        // EF Core/Npgsql: use EF.Functions or cast. We load minimal fields and group in-memory
        // to avoid EF translation issues with TimeSpan arithmetic.
        var txRows = await txQuery
            .Select(t => new
            {
                t.CompletedAt,
                t.FinalAmount,
                t.DiscountAmount,
                t.TaxAmount,
            })
            .ToListAsync(cancellationToken);

        var dailyBreakdown = txRows
            .GroupBy(t => DateOnly.FromDateTime(t.CompletedAt!.Value + ManilaOffset))
            .OrderBy(g => g.Key)
            .Select(g => new RevenueDayDto(
                g.Key,
                g.Sum(t => t.FinalAmount),
                g.Sum(t => t.DiscountAmount),
                g.Sum(t => t.TaxAmount),
                g.Count()))
            .ToList();

        var grandTotal       = txRows.Sum(t => t.FinalAmount);
        var totalDiscount    = txRows.Sum(t => t.DiscountAmount);
        var totalTax         = txRows.Sum(t => t.TaxAmount);
        var transactionCount = txRows.Count;

        // ── Payment method breakdown ──────────────────────────────────────────
        // Load transaction IDs first, then join payments (avoids complex cross-join).
        var txIds = txRows.Count > 0
            ? txRows.Select(_ => _.CompletedAt).ToList() // placeholder — use real join below
            : null;

        var paymentQuery = context.Payments
            .AsNoTracking()
            .Where(p =>
                p.Transaction.Status == TransactionStatus.Completed &&
                p.Transaction.CompletedAt >= fromUtc &&
                p.Transaction.CompletedAt < toUtc);

        if (request.BranchId is not null)
            paymentQuery = paymentQuery.Where(p => p.Transaction.BranchId == request.BranchId);

        var byPaymentMethod = (await paymentQuery
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new
            {
                Method = g.Key,
                Amount = g.Sum(p => p.Amount),
                Count  = g.Count(),
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync(cancellationToken))
            .Select(x => new RevenueByPaymentMethodDto(
                x.Method.ToString(),
                x.Amount,
                x.Count))
            .ToList();

        return new RevenueReportDto(
            request.From,
            request.To,
            request.BranchId,
            branchName,
            grandTotal,
            totalDiscount,
            totalTax,
            transactionCount,
            dailyBreakdown,
            byPaymentMethod);
    }
}
