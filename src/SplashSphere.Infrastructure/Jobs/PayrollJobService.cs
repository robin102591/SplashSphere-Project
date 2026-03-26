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
/// their <see cref="PayrollSettings.Frequency"/> and <see cref="PayrollSettings.CutOffStartDay"/>.
/// <list type="bullet">
///   <item>Auto-closes expired Open periods whose EndDate has passed.</item>
///   <item>Creates new periods: weekly (on CutOffStartDay) or semi-monthly (on 1st and 16th).</item>
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
            .ToDictionaryAsync(
                ps => ps.TenantId,
                ps => (ps.CutOffStartDay, ps.Frequency),
                ct);

        // ── 3. Auto-close expired Open periods (any tenant, any day) ──────────
        await AutoCloseExpiredPeriodsAsync(todayManila, ct);

        // ── 4. Create new periods per tenant ────────────────────────────────
        var created = 0;

        // Pre-load existing period start dates for duplicate prevention
        // Check both possible start dates: previous week and semi-monthly boundaries
        var possibleStartDates = ComputePossibleStartDates(todayManila);
        var existingTenants = await db.PayrollPeriods
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => possibleStartDates.Contains(p.StartDate))
            .Select(p => new { p.TenantId, p.StartDate })
            .ToListAsync(ct);

        var existingSet = existingTenants
            .Select(x => $"{x.TenantId}|{x.StartDate}")
            .ToHashSet();

        foreach (var tenantId in tenantIds)
        {
            var (cutOffDay, frequency) = settingsMap.TryGetValue(tenantId, out var settings)
                ? settings
                : (DayOfWeek.Monday, PayrollFrequency.Weekly);

            DateOnly startDate, endDate;
            int cutOffWeek;

            if (frequency == PayrollFrequency.SemiMonthly)
            {
                // Semi-monthly: create on the 1st (for prev month 16th–last) and 16th (for 1st–15th)
                if (todayManila.Day == 16)
                {
                    // Period covers 1st–15th of current month
                    startDate = new DateOnly(todayManila.Year, todayManila.Month, 1);
                    endDate   = new DateOnly(todayManila.Year, todayManila.Month, 15);
                }
                else if (todayManila.Day == 1)
                {
                    // Period covers 16th–last day of previous month
                    var prevMonth = todayManila.AddMonths(-1);
                    startDate = new DateOnly(prevMonth.Year, prevMonth.Month, 16);
                    endDate   = new DateOnly(prevMonth.Year, prevMonth.Month,
                        DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month));
                }
                else
                {
                    continue; // Not a semi-monthly trigger day
                }

                cutOffWeek = ComputeSemiMonthlyCutOffWeek(startDate);
            }
            else
            {
                // Weekly: create on CutOffStartDay for previous 7 days
                if (todayDow != cutOffDay)
                    continue;

                startDate  = todayManila.AddDays(-7);
                endDate    = todayManila.AddDays(-1);
                cutOffWeek = ComputeWeeklyCutOffWeek(startDate, cutOffDay);
            }

            var key = $"{tenantId}|{startDate}";
            if (existingSet.Contains(key))
            {
                logger.LogDebug(
                    "PayrollJob: Tenant {TenantId} already has a period starting {Date} — skipping.",
                    tenantId, startDate);
                continue;
            }

            var year = startDate.Year;
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
    /// Computes all possible period start dates for today to check duplicates in a single query.
    /// </summary>
    private static List<DateOnly> ComputePossibleStartDates(DateOnly todayManila)
    {
        var dates = new List<DateOnly>
        {
            // Weekly: always previous 7 days
            todayManila.AddDays(-7),
        };

        // Semi-monthly: 1st–15th or 16th–last
        if (todayManila.Day == 16)
            dates.Add(new DateOnly(todayManila.Year, todayManila.Month, 1));
        else if (todayManila.Day == 1)
        {
            var prevMonth = todayManila.AddMonths(-1);
            dates.Add(new DateOnly(prevMonth.Year, prevMonth.Month, 16));
        }

        return dates;
    }

    /// <summary>
    /// Computes a sequential week number relative to the tenant's configured
    /// cut-off start day within the year.
    /// </summary>
    private static int ComputeWeeklyCutOffWeek(DateOnly startDate, DayOfWeek cutOffStartDay)
    {
        var jan1 = new DateOnly(startDate.Year, 1, 1);
        var daysUntilStart = ((int)cutOffStartDay - (int)jan1.DayOfWeek + 7) % 7;
        var firstCutOff = jan1.AddDays(daysUntilStart);

        if (startDate < firstCutOff)
            return 1;

        return ((startDate.DayNumber - firstCutOff.DayNumber) / 7) + 1;
    }

    /// <summary>
    /// Computes a period number (1–24) for semi-monthly payroll.
    /// January 1st–15th = 1, January 16th–31st = 2, February 1st–15th = 3, etc.
    /// </summary>
    private static int ComputeSemiMonthlyCutOffWeek(DateOnly startDate)
    {
        return (startDate.Month - 1) * 2 + (startDate.Day <= 15 ? 1 : 2);
    }
}
