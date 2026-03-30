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
/// Runs once per day (00:05 PHT) and processes each tenant's branches according to
/// their effective <see cref="PayrollSettings"/> (branch override → tenant default).
/// <list type="bullet">
///   <item>Auto-closes expired Open periods whose EndDate has passed.</item>
///   <item>Creates new per-branch periods: weekly (on CutOffStartDay) or semi-monthly (on 1st and 16th).</item>
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

        // ── 1. Load all active tenants and their branches ────────────────────
        var tenantBranches = await db.Branches
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(b => b.IsActive && b.Tenant.IsActive)
            .Select(b => new { b.TenantId, BranchId = b.Id })
            .ToListAsync(ct);

        var tenantIds = tenantBranches.Select(x => x.TenantId).Distinct().ToList();

        // ── 2. Load all payroll settings (tenant defaults + branch overrides) ─
        var allSettings = await db.PayrollSettings
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(ps => tenantIds.Contains(ps.TenantId))
            .Select(ps => new { ps.TenantId, ps.BranchId, ps.CutOffStartDay, ps.Frequency })
            .ToListAsync(ct);

        var settingsMap = allSettings.ToDictionary(
            s => (s.TenantId, s.BranchId ?? ""),
            s => (s.CutOffStartDay, s.Frequency));

        // ── 3. Auto-close expired Open periods (any tenant/branch, any day) ──
        await AutoCloseExpiredPeriodsAsync(todayManila, ct);

        // ── 4. Create new per-branch periods ─────────────────────────────────
        var created = 0;

        // Pre-load existing period start dates for duplicate prevention
        var possibleStartDates = ComputePossibleStartDates(todayManila);
        var existingPeriods = await db.PayrollPeriods
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => possibleStartDates.Contains(p.StartDate))
            .Select(p => new { p.TenantId, p.BranchId, p.StartDate })
            .ToListAsync(ct);

        var existingSet = existingPeriods
            .Select(x => $"{x.TenantId}|{x.BranchId ?? ""}|{x.StartDate}")
            .ToHashSet();

        foreach (var tb in tenantBranches)
        {
            // Resolve effective settings: branch override → tenant default → hardcoded
            var (cutOffDay, frequency) = ResolveEffectiveSettings(settingsMap, tb.TenantId, tb.BranchId);

            DateOnly startDate, endDate;
            int cutOffWeek;

            if (frequency == PayrollFrequency.SemiMonthly)
            {
                if (todayManila.Day == 16)
                {
                    startDate = new DateOnly(todayManila.Year, todayManila.Month, 1);
                    endDate   = new DateOnly(todayManila.Year, todayManila.Month, 15);
                }
                else if (todayManila.Day == 1)
                {
                    var prevMonth = todayManila.AddMonths(-1);
                    startDate = new DateOnly(prevMonth.Year, prevMonth.Month, 16);
                    endDate   = new DateOnly(prevMonth.Year, prevMonth.Month,
                        DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month));
                }
                else
                {
                    continue;
                }

                cutOffWeek = ComputeSemiMonthlyCutOffWeek(startDate);
            }
            else
            {
                if (todayDow != cutOffDay)
                    continue;

                startDate  = todayManila.AddDays(-7);
                endDate    = todayManila.AddDays(-1);
                cutOffWeek = ComputeWeeklyCutOffWeek(startDate, cutOffDay);
            }

            var key = $"{tb.TenantId}|{tb.BranchId}|{startDate}";
            if (existingSet.Contains(key))
            {
                logger.LogDebug(
                    "PayrollJob: Tenant {TenantId} branch {BranchId} already has a period starting {Date} — skipping.",
                    tb.TenantId, tb.BranchId, startDate);
                continue;
            }

            var year = startDate.Year;
            db.PayrollPeriods.Add(new PayrollPeriod(tb.TenantId, year, cutOffWeek, startDate, endDate, tb.BranchId));
            created++;
        }

        if (created > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "PayrollJob: Daily run complete. Created {Created} new period(s).", created);
    }

    // ── Resolve effective settings ──────────────────────────────────────────

    private static (DayOfWeek CutOffDay, PayrollFrequency Frequency) ResolveEffectiveSettings(
        Dictionary<(string, string), (DayOfWeek, PayrollFrequency)> settingsMap,
        string tenantId,
        string branchId)
    {
        // Branch override
        if (settingsMap.TryGetValue((tenantId, branchId), out var branchSettings))
            return branchSettings;

        // Tenant default
        if (settingsMap.TryGetValue((tenantId, ""), out var tenantSettings))
            return tenantSettings;

        // Hardcoded default
        return (DayOfWeek.Monday, PayrollFrequency.Weekly);
    }

    // ── Auto-close expired periods ──────────────────────────────────────────

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

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static List<DateOnly> ComputePossibleStartDates(DateOnly todayManila)
    {
        var dates = new List<DateOnly>
        {
            todayManila.AddDays(-7),
        };

        if (todayManila.Day == 16)
            dates.Add(new DateOnly(todayManila.Year, todayManila.Month, 1));
        else if (todayManila.Day == 1)
        {
            var prevMonth = todayManila.AddMonths(-1);
            dates.Add(new DateOnly(prevMonth.Year, prevMonth.Month, 16));
        }

        return dates;
    }

    private static int ComputeWeeklyCutOffWeek(DateOnly startDate, DayOfWeek cutOffStartDay)
    {
        var jan1 = new DateOnly(startDate.Year, 1, 1);
        var daysUntilStart = ((int)cutOffStartDay - (int)jan1.DayOfWeek + 7) % 7;
        var firstCutOff = jan1.AddDays(daysUntilStart);

        if (startDate < firstCutOff)
            return 1;

        return ((startDate.DayNumber - firstCutOff.DayNumber) / 7) + 1;
    }

    private static int ComputeSemiMonthlyCutOffWeek(DateOnly startDate)
    {
        return (startDate.Month - 1) * 2 + (startDate.Day <= 15 ? 1 : 2);
    }
}
