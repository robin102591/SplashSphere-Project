using System.Globalization;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Payroll.Commands.ClosePayrollPeriod;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Infrastructure.Auth;

namespace SplashSphere.Infrastructure.Jobs;

/// <summary>
/// Recurring Hangfire jobs for payroll lifecycle management.
/// <list type="bullet">
///   <item><see cref="CreateWeeklyPayrollPeriodsAsync"/> — Monday 00:00 PHT.
///   Creates one <see cref="PayrollPeriod"/> per tenant for the current ISO week.
///   Idempotent: skips tenants that already have a period for this week.</item>
///   <item><see cref="AutoCloseExpiredPeriodsAsync"/> — Sunday 23:55 PHT.
///   Closes all Open periods whose <c>EndDate</c> is before today (Manila).
///   Delegates to <see cref="ClosePayrollPeriodCommand"/> so payroll-entry creation
///   logic is not duplicated.</item>
/// </list>
/// </summary>
public sealed class PayrollJobService(
    IServiceScopeFactory scopeFactory,
    ILogger<PayrollJobService> logger)
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    // ── Create weekly periods ─────────────────────────────────────────────────

    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 120)]
    public async Task CreateWeeklyPayrollPeriodsAsync(CancellationToken ct = default)
    {
        var nowManila   = DateTime.UtcNow + ManilaOffset;
        var todayManila = DateOnly.FromDateTime(nowManila);

        // ISO week boundaries for the current week (Monday–Sunday).
        var isoYear = ISOWeek.GetYear(nowManila);
        var isoWeek = ISOWeek.GetWeekOfYear(nowManila);
        var monday  = DateOnly.FromDateTime(ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday));
        var sunday  = monday.AddDays(6);

        logger.LogInformation(
            "PayrollJob: Creating weekly periods for ISO {Year}-W{Week:D2} ({Monday}–{Sunday}).",
            isoYear, isoWeek, monday, sunday);

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Load all tenant IDs (cross-tenant scan, bypasses global filter).
        var tenantIds = await db.Tenants
            .AsNoTracking()
            .Where(t => t.IsActive)
            .Select(t => t.Id)
            .ToListAsync(ct);

        // Load existing period keys for this week in one query.
        var existing = await db.PayrollPeriods
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.Year == isoYear && p.CutOffWeek == isoWeek)
            .Select(p => p.TenantId)
            .ToListAsync(ct);

        var existingSet = existing.ToHashSet();
        var created = 0;

        foreach (var tenantId in tenantIds)
        {
            if (existingSet.Contains(tenantId))
            {
                logger.LogDebug(
                    "PayrollJob: Tenant {TenantId} already has a period for W{Week} — skipping.",
                    tenantId, isoWeek);
                continue;
            }

            db.PayrollPeriods.Add(new PayrollPeriod(tenantId, isoYear, isoWeek, monday, sunday));
            created++;
        }

        if (created > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "PayrollJob: Created {Created} new payroll period(s) for W{Week}.",
            created, isoWeek);
    }

    // ── Auto-close expired periods ────────────────────────────────────────────

    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task AutoCloseExpiredPeriodsAsync(CancellationToken ct = default)
    {
        var todayManila = DateOnly.FromDateTime(DateTime.UtcNow + ManilaOffset);

        logger.LogInformation(
            "PayrollJob: Auto-closing expired Open periods (EndDate < {Today}).", todayManila);

        // Cross-tenant scan: find Open periods whose EndDate has passed.
        List<(string Id, string TenantId)> expired;
        using (var scanScope = scopeFactory.CreateScope())
        {
            var db = scanScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            expired = await db.PayrollPeriods
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(p => p.Status == PayrollStatus.Open && p.EndDate < todayManila)
                .Select(p => new { p.Id, p.TenantId })
                .ToListAsync(ct)
                .ContinueWith(t => t.Result.Select(x => (x.Id, x.TenantId)).ToList(), ct);
        }

        if (expired.Count == 0)
        {
            logger.LogInformation("PayrollJob: No expired Open periods found.");
            return;
        }

        logger.LogInformation("PayrollJob: Closing {Count} expired period(s).", expired.Count);

        var closed  = 0;
        var failed  = 0;

        foreach (var (periodId, tenantId) in expired)
        {
            // Create a dedicated scope per tenant so the global query filter and
            // PayrollEntry construction both see the correct TenantId.
            using var tenantScope = scopeFactory.CreateScope();

            var tenantCtx = tenantScope.ServiceProvider.GetRequiredService<TenantContext>();
            tenantCtx.TenantId = tenantId;

            var mediator = tenantScope.ServiceProvider.GetRequiredService<MediatR.ISender>();

            try
            {
                var result = await mediator.Send(new ClosePayrollPeriodCommand(periodId), ct);

                if (result.IsSuccess)
                {
                    closed++;
                    logger.LogInformation(
                        "PayrollJob: Closed period {PeriodId} for tenant {TenantId}.",
                        periodId, tenantId);
                }
                else
                {
                    failed++;
                    logger.LogWarning(
                        "PayrollJob: Failed to close period {PeriodId} for tenant {TenantId}: {Error}",
                        periodId, tenantId, result.Error);
                }
            }
            catch (Exception ex)
            {
                failed++;
                logger.LogError(ex,
                    "PayrollJob: Exception closing period {PeriodId} for tenant {TenantId}.",
                    periodId, tenantId);
            }
        }

        logger.LogInformation(
            "PayrollJob: Auto-close complete. Closed={Closed}, Failed={Failed}.",
            closed, failed);
    }
}
