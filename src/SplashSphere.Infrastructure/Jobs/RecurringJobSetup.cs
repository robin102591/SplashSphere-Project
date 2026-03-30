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

        // Clean up old weekly jobs (replaced by daily job)
        manager.RemoveIfExists("payroll-create-weekly");
        manager.RemoveIfExists("payroll-auto-close");

        // Daily 00:05 PHT — per-tenant payroll: auto-close expired periods,
        // then create new periods for tenants whose CutOffStartDay matches today.
        manager.AddOrUpdate<PayrollJobService>(
            recurringJobId: "payroll-daily",
            methodCall: job => job.RunDailyPayrollJobAsync(CancellationToken.None),
            cronExpression: Cron.Daily(hour: 0, minute: 5),
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

        // ── Billing ──────────────────────────────────────────────────────────

        // Daily 9 AM PHT — remind tenants with trials expiring in 1-3 days.
        manager.AddOrUpdate<BillingJobService>(
            recurringJobId: "billing-trial-reminder",
            methodCall: job => job.SendTrialExpiryReminderAsync(CancellationToken.None),
            cronExpression: Cron.Daily(hour: 9),
            options: new RecurringJobOptions { TimeZone = Manila });

        // Daily 9 AM PHT — suspend accounts that are PastDue for > 7 days.
        manager.AddOrUpdate<BillingJobService>(
            recurringJobId: "billing-suspend-overdue",
            methodCall: job => job.SuspendOverdueAccountsAsync(CancellationToken.None),
            cronExpression: Cron.Daily(hour: 9, minute: 5),
            options: new RecurringJobOptions { TimeZone = Manila });

        // 1st of each month 00:30 PHT — reset SMS counters.
        manager.AddOrUpdate<BillingJobService>(
            recurringJobId: "billing-sms-reset",
            methodCall: job => job.ResetMonthlySmsCountAsync(CancellationToken.None),
            cronExpression: "30 0 1 * *",
            options: new RecurringJobOptions { TimeZone = Manila });

        // 1st of each month 01:00 PHT — generate invoice records.
        manager.AddOrUpdate<BillingJobService>(
            recurringJobId: "billing-monthly-invoices",
            methodCall: job => job.GenerateMonthlyInvoicesAsync(CancellationToken.None),
            cronExpression: "0 1 1 * *",
            options: new RecurringJobOptions { TimeZone = Manila });

        return app;
    }
}
