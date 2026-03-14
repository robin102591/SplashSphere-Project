using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SplashSphere.Infrastructure.Jobs;

/// <summary>
/// Registers all recurring Hangfire jobs using the Asia/Manila timezone (UTC+8).
/// Call <see cref="UseRecurringJobs"/> after the Hangfire middleware is in place.
/// <para>
/// <b>Timezone note:</b> Hangfire persists cron schedules with a timezone.
/// On Linux (Docker), "Asia/Manila" resolves correctly via the IANA tz database.
/// On Windows development machines, the IANA ID is not natively available so we
/// fall back to "Singapore Standard Time" (also UTC+8, the closest Windows alias).
/// </para>
/// </summary>
public static class RecurringJobSetup
{
    /// <summary>
    /// IANA / Windows timezone ID for Asia/Manila (UTC+8).
    /// Uses the IANA form on Linux/macOS (Docker) and the Windows alias on Windows dev machines.
    /// </summary>
    private static readonly TimeZoneInfo Manila =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Singapore Standard Time" : "Asia/Manila");

    public static IApplicationBuilder UseRecurringJobs(this IApplicationBuilder app)
    {
        var manager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();

        // ── Payroll ───────────────────────────────────────────────────────────

        // Mon 00:00 PHT — create one period per tenant for the new ISO week.
        manager.AddOrUpdate<PayrollJobService>(
            recurringJobId: "payroll-create-weekly",
            methodCall: job => job.CreateWeeklyPayrollPeriodsAsync(CancellationToken.None),
            cronExpression: Cron.Weekly(DayOfWeek.Monday, hour: 0, minute: 0),
            options: new RecurringJobOptions { TimeZone = Manila });

        // Sun 23:55 PHT — close all Open periods whose EndDate has passed.
        manager.AddOrUpdate<PayrollJobService>(
            recurringJobId: "payroll-auto-close",
            methodCall: job => job.AutoCloseExpiredPeriodsAsync(CancellationToken.None),
            cronExpression: "55 23 * * 0",          // 23:55 every Sunday
            options: new RecurringJobOptions { TimeZone = Manila });

        // ── Inventory ─────────────────────────────────────────────────────────

        // Daily 08:00 PHT — publish LowStockAlertEvent for items at or below threshold.
        manager.AddOrUpdate<InventoryJobService>(
            recurringJobId: "inventory-low-stock-check",
            methodCall: job => job.CheckLowStockAlertsAsync(CancellationToken.None),
            cronExpression: Cron.Daily(hour: 8),
            options: new RecurringJobOptions { TimeZone = Manila });

        // ── Transactions ──────────────────────────────────────────────────────

        // Every hour — cancel Pending transactions older than 4 hours.
        manager.AddOrUpdate<TransactionJobService>(
            recurringJobId: "transaction-stale-cleanup",
            methodCall: job => job.CancelStalePendingTransactionsAsync(CancellationToken.None),
            cronExpression: Cron.Hourly(),
            options: new RecurringJobOptions { TimeZone = Manila });

        // ── Queue ─────────────────────────────────────────────────────────────

        // Every 5 minutes — safety-net sweep for Called entries > 5 min without service starting.
        manager.AddOrUpdate<QueueJobService>(
            recurringJobId: "queue-noshow-sweep",
            methodCall: job => job.MarkStuckNoShowsAsync(CancellationToken.None),
            cronExpression: "*/5 * * * *",
            options: new RecurringJobOptions { TimeZone = Manila });

        return app;
    }
}
