using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Dashboard.Queries.GetDashboardSummary;

public sealed class GetDashboardSummaryQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<DashboardSummaryDto> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        // ── Manila calendar windows ───────────────────────────────────────────
        var nowManila   = DateTime.UtcNow + ManilaOffset;
        var todayManila = DateOnly.FromDateTime(nowManila);

        // Today: midnight–midnight Manila → UTC
        var todayStart = todayManila.ToDateTime(TimeOnly.MinValue) - ManilaOffset;
        var todayEnd   = todayStart.AddDays(1);

        // This week: Monday → Sunday (ISO week)
        var daysSinceMonday = ((int)nowManila.DayOfWeek + 6) % 7;
        var weekStart = todayManila.AddDays(-daysSinceMonday).ToDateTime(TimeOnly.MinValue) - ManilaOffset;
        var weekEnd   = weekStart.AddDays(7);

        // This month: first day of month
        var monthStart = new DateTime(nowManila.Year, nowManila.Month, 1) - ManilaOffset;
        var monthEnd   = monthStart.AddMonths(1);

        // ── Base transaction predicate ────────────────────────────────────────
        var txBase = context.Transactions
            .AsNoTracking()
            .Where(t => t.Status == TransactionStatus.Completed);

        if (request.BranchId is not null)
            txBase = txBase.Where(t => t.BranchId == request.BranchId);

        // ── Revenue & transaction counts ──────────────────────────────────────
        // Three focused queries — avoids single complex query with multiple aggregates.
        var revenueToday = await txBase
            .Where(t => t.CompletedAt >= todayStart && t.CompletedAt < todayEnd)
            .SumAsync(t => t.FinalAmount, cancellationToken);

        var revenueWeek = await txBase
            .Where(t => t.CompletedAt >= weekStart && t.CompletedAt < weekEnd)
            .SumAsync(t => t.FinalAmount, cancellationToken);

        var revenueMonth = await txBase
            .Where(t => t.CompletedAt >= monthStart && t.CompletedAt < monthEnd)
            .SumAsync(t => t.FinalAmount, cancellationToken);

        var txToday = await txBase
            .CountAsync(t => t.CompletedAt >= todayStart && t.CompletedAt < todayEnd, cancellationToken);

        var txWeek = await txBase
            .CountAsync(t => t.CompletedAt >= weekStart && t.CompletedAt < weekEnd, cancellationToken);

        var txMonth = await txBase
            .CountAsync(t => t.CompletedAt >= monthStart && t.CompletedAt < monthEnd, cancellationToken);

        // ── Queue (current snapshot — not time-bounded) ───────────────────────
        var queueBase = context.QueueEntries.AsNoTracking();

        if (request.BranchId is not null)
            queueBase = queueBase.Where(q => q.BranchId == request.BranchId);

        var queueWaiting   = await queueBase.CountAsync(q => q.Status == QueueStatus.Waiting,   cancellationToken);
        var queueInService = await queueBase.CountAsync(q => q.Status == QueueStatus.InService,  cancellationToken);

        // ── Workforce ─────────────────────────────────────────────────────────
        var employeeBase = context.Employees.AsNoTracking().Where(e => e.IsActive);

        if (request.BranchId is not null)
            employeeBase = employeeBase.Where(e => e.BranchId == request.BranchId);

        var activeEmployees = await employeeBase.CountAsync(cancellationToken);

        var clockedInToday = await context.Attendances
            .AsNoTracking()
            .CountAsync(a =>
                (request.BranchId == null || a.Employee.BranchId == request.BranchId) &&
                a.Date == todayManila,
                cancellationToken);

        // ── Branch breakdowns (tenant-wide only) ──────────────────────────────
        IReadOnlyList<BranchKpiDto>? branches = null;

        if (request.BranchId is null)
        {
            // Load branch IDs + names in one query.
            var branchList = await context.Branches
                .AsNoTracking()
                .Where(b => b.IsActive)
                .Select(b => new { b.Id, b.Name })
                .ToListAsync(cancellationToken);

            // Per-branch revenue today — single grouped query.
            var branchRevenue = await context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.Status == TransactionStatus.Completed &&
                    t.CompletedAt >= todayStart &&
                    t.CompletedAt < todayEnd)
                .GroupBy(t => t.BranchId)
                .Select(g => new { BranchId = g.Key, Revenue = g.Sum(t => t.FinalAmount), Count = g.Count() })
                .ToListAsync(cancellationToken);

            // Per-branch queue today — single grouped query.
            var branchQueue = await context.QueueEntries
                .AsNoTracking()
                .Where(q => q.Status == QueueStatus.Waiting || q.Status == QueueStatus.InService)
                .GroupBy(q => q.BranchId)
                .Select(g => new
                {
                    BranchId  = g.Key,
                    Waiting   = g.Count(q => q.Status == QueueStatus.Waiting),
                    InService = g.Count(q => q.Status == QueueStatus.InService),
                })
                .ToListAsync(cancellationToken);

            branches = branchList
                .Select(b =>
                {
                    var rev   = branchRevenue.FirstOrDefault(r => r.BranchId == b.Id);
                    var queue = branchQueue.FirstOrDefault(q => q.BranchId == b.Id);
                    return new BranchKpiDto(
                        b.Id,
                        b.Name,
                        rev?.Revenue ?? 0m,
                        rev?.Count   ?? 0,
                        queue?.Waiting   ?? 0,
                        queue?.InService ?? 0);
                })
                .ToList();
        }

        return new DashboardSummaryDto(
            revenueToday,
            revenueWeek,
            revenueMonth,
            txToday,
            txWeek,
            txMonth,
            queueWaiting,
            queueInService,
            activeEmployees,
            clockedInToday,
            branches);
    }
}
