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
/// Daily Hangfire job for payroll lifecycle management.
/// Runs once per day (00:05 PHT) and processes each tenant according to
/// their <see cref="PayrollSettings.CutOffStartDay"/>.
/// <list type="bullet">
///   <item>Auto-closes expired Open periods whose EndDate has passed.</item>
///   <item>Creates a new period for tenants whose CutOffStartDay matches today.</item>
/// </list>
/// </summary>
public sealed class PayrollJobService(
    IServiceScopeFactory scopeFactory,
    ILogger<PayrollJobService> logger)
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task RunDailyPayrollJobAsync(CancellationToken ct = default)
    {
        var todayManila = DateOnly.FromDateTime(DateTime.UtcNow + ManilaOffset);
        var todayDow    = todayManila.DayOfWeek;

        logger.LogInformation(
            "PayrollJob: Daily run for {Today} ({DayOfWeek}).", todayManila, todayDow);

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // ── 1. Load all active tenants ────────────────────────────────────────
        var tenantIds = await db.Tenants
            .AsNoTracking()
            .Where(t => t.IsActive)
            .Select(t => t.Id)
            .ToListAsync(ct);

        // ── 2. Load per-tenant payroll settings ───────────────────────────────
        var settingsMap = await db.PayrollSettings
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(ps => tenantIds.Contains(ps.TenantId))
            .ToDictionaryAsync(ps => ps.TenantId, ps => ps.CutOffStartDay, ct);

        // ── 3. Auto-close expired Open periods (any tenant, any day) ──────────
        await AutoCloseExpiredPeriodsAsync(todayManila, ct);

        // ── 4. Create new periods for tenants whose start day is today ────────
        var created = 0;

        // The cut-off day is the day AFTER the period ends.
        // If today is the cut-off day, the period covers the previous 7 days:
        //   startDate = today - 7, endDate = today - 1
        // Example: cut-off Thursday March 26 → period March 19 (Thu) – March 25 (Wed)

        // Load existing periods for the previous week's start date (prevents duplicates)
        var previousWeekStart = todayManila.AddDays(-7);
        var existingStartDates = await db.PayrollPeriods
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.StartDate == previousWeekStart)
            .Select(p => p.TenantId)
            .ToListAsync(ct);

        var existingSet = existingStartDates.ToHashSet();

        foreach (var tenantId in tenantIds)
        {
            var cutOffDay = settingsMap.GetValueOrDefault(tenantId, DayOfWeek.Monday);

            if (todayDow != cutOffDay)
                continue;

            var startDate = todayManila.AddDays(-7);
            var endDate   = todayManila.AddDays(-1);

            if (existingSet.Contains(tenantId))
            {
                logger.LogDebug(
                    "PayrollJob: Tenant {TenantId} already has a period starting {Date} — skipping.",
                    tenantId, startDate);
                continue;
            }

            var year       = startDate.Year;
            var cutOffWeek = ComputeCutOffWeek(startDate, cutOffDay);

            db.PayrollPeriods.Add(new PayrollPeriod(tenantId, year, cutOffWeek, startDate, endDate));
            created++;
        }

        if (created > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "PayrollJob: Daily run complete. Created {Created} new period(s).", created);
    }

    // ── Auto-close expired periods ────────────────────────────────────────────

    private async Task AutoCloseExpiredPeriodsAsync(DateOnly todayManila, CancellationToken ct)
    {
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
            logger.LogDebug("PayrollJob: No expired Open periods found.");
            return;
        }

        logger.LogInformation("PayrollJob: Closing {Count} expired period(s).", expired.Count);

        var closed = 0;
        var failed = 0;

        foreach (var (periodId, tenantId) in expired)
        {
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

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Computes a sequential week number relative to the tenant's configured
    /// cut-off start day within the year.
    /// </summary>
    private static int ComputeCutOffWeek(DateOnly startDate, DayOfWeek cutOffStartDay)
    {
        var jan1 = new DateOnly(startDate.Year, 1, 1);
        var daysUntilStart = ((int)cutOffStartDay - (int)jan1.DayOfWeek + 7) % 7;
        var firstCutOff = jan1.AddDays(daysUntilStart);

        if (startDate < firstCutOff)
            return 1;

        return ((startDate.DayNumber - firstCutOff.DayNumber) / 7) + 1;
    }
}
