using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Reports.Queries.GetCustomerAnalytics;

public sealed class GetCustomerAnalyticsQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetCustomerAnalyticsQuery, CustomerAnalyticsDto>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<CustomerAnalyticsDto> Handle(
        GetCustomerAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        var fromUtc = DateTime.SpecifyKind(request.From.ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(request.To.AddDays(1).ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);

        // ── Completed transactions in range ─────────────────────────────────
        var txQuery = context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == TransactionStatus.Completed &&
                t.CompletedAt >= fromUtc &&
                t.CompletedAt < toUtc &&
                t.CustomerId != null);

        if (request.BranchId is not null)
            txQuery = txQuery.Where(t => t.BranchId == request.BranchId);

        var txRows = await txQuery
            .Select(t => new
            {
                t.CustomerId,
                t.CompletedAt,
                t.FinalAmount,
                t.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        // ── Determine "new" vs "returning" ──────────────────────────────────
        // A customer is "new" if their first-ever completed transaction falls within the range.
        var customerIds = txRows.Select(t => t.CustomerId!).Distinct().ToList();

        // First transaction date per customer (may be before the range)
        var firstTxBase = context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Status == TransactionStatus.Completed &&
                t.CustomerId != null &&
                t.CustomerId != null &&
                customerIds.Contains(t.CustomerId));

        if (request.BranchId is not null)
            firstTxBase = firstTxBase.Where(t => t.BranchId == request.BranchId);

        var firstTxDates = await firstTxBase
            .GroupBy(t => t.CustomerId!)
            .Select(g => new { CustomerId = g.Key, FirstTx = g.Min(t => t.CompletedAt) })
            .ToListAsync(cancellationToken);

        var firstTxLookup = firstTxDates.ToDictionary(x => x.CustomerId, x => x.FirstTx);

        var newCustomerIds = customerIds
            .Where(id => firstTxLookup.TryGetValue(id, out var first) && first >= fromUtc)
            .ToHashSet();

        var newCustomers = newCustomerIds.Count;
        var returningCustomers = customerIds.Count - newCustomers;
        var totalCustomers = customerIds.Count;

        // ── Retention rate ──────────────────────────────────────────────────
        // % of customers who had transactions before this period AND also in this period
        var customersBeforePeriod = firstTxLookup
            .Count(kvp => kvp.Value < fromUtc);
        var retainedCustomers = customerIds
            .Count(id => firstTxLookup.TryGetValue(id, out var first) && first < fromUtc);
        var retentionRate = customersBeforePeriod > 0
            ? Math.Round((decimal)retainedCustomers / customersBeforePeriod * 100, 1)
            : 0m;

        // ── Visit frequency ─────────────────────────────────────────────────
        var visitsPerCustomer = txRows
            .GroupBy(t => t.CustomerId!)
            .Select(g => g.Count())
            .ToList();

        var avgVisits = visitsPerCustomer.Count > 0
            ? Math.Round((decimal)visitsPerCustomer.Average(), 1)
            : 0m;

        var avgSpendPerVisit = txRows.Count > 0
            ? Math.Round(txRows.Average(t => t.FinalAmount), 2)
            : 0m;

        // Visit frequency buckets
        var frequencyBuckets = new List<VisitFrequencyBucketDto>
        {
            new("1 visit", visitsPerCustomer.Count(v => v == 1)),
            new("2-3 visits", visitsPerCustomer.Count(v => v >= 2 && v <= 3)),
            new("4-6 visits", visitsPerCustomer.Count(v => v >= 4 && v <= 6)),
            new("7-10 visits", visitsPerCustomer.Count(v => v >= 7 && v <= 10)),
            new("11+ visits", visitsPerCustomer.Count(v => v > 10)),
        };

        // ── Top customers ───────────────────────────────────────────────────
        var customerNames = await context.Customers
            .AsNoTracking()
            .Where(c => customerIds.Contains(c.Id))
            .Select(c => new { c.Id, Name = (c.FirstName + " " + c.LastName).Trim() })
            .ToListAsync(cancellationToken);

        var customerNameLookup = customerNames.ToDictionary(c => c.Id, c => c.Name);

        // Get primary plate numbers for top customers
        var customerPlates = await context.Cars
            .AsNoTracking()
            .Where(c => c.CustomerId != null && customerIds.Contains(c.CustomerId))
            .GroupBy(c => c.CustomerId!)
            .Select(g => new { CustomerId = g.Key, Plate = g.OrderByDescending(c => c.CreatedAt).Select(c => c.PlateNumber).FirstOrDefault() })
            .ToListAsync(cancellationToken);

        var plateLookup = customerPlates.ToDictionary(c => c.CustomerId, c => c.Plate);

        var topCustomers = txRows
            .GroupBy(t => t.CustomerId!)
            .Select(g =>
            {
                var lastVisitUtc = g.Max(t => t.CompletedAt!.Value);
                return new TopCustomerDto(
                    g.Key,
                    customerNameLookup.GetValueOrDefault(g.Key, "Unknown"),
                    plateLookup.GetValueOrDefault(g.Key),
                    g.Count(),
                    g.Sum(t => t.FinalAmount),
                    Math.Round(g.Average(t => t.FinalAmount), 2),
                    DateOnly.FromDateTime(lastVisitUtc + ManilaOffset));
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(20)
            .ToList();

        // ── Daily trend ─────────────────────────────────────────────────────
        var dailyTrend = txRows
            .GroupBy(t => DateOnly.FromDateTime(t.CompletedAt!.Value + ManilaOffset))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var dayCustomerIds = g.Select(t => t.CustomerId!).Distinct().ToList();
                var dayNew = dayCustomerIds.Count(id => newCustomerIds.Contains(id));
                return new CustomerTrendDayDto(
                    g.Key,
                    dayNew,
                    dayCustomerIds.Count - dayNew,
                    g.Count());
            })
            .ToList();

        return new CustomerAnalyticsDto(
            request.From,
            request.To,
            request.BranchId,
            totalCustomers,
            newCustomers,
            returningCustomers,
            retentionRate,
            avgVisits,
            avgSpendPerVisit,
            topCustomers,
            frequencyBuckets,
            dailyTrend);
    }
}
