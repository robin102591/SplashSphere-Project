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

        // Every 6 hours — scan merchandise and supply items for low stock alerts.
        manager.AddOrUpdate<InventoryJobService>(
            recurringJobId: "inventory-low-stock-check",
            methodCall: job => job.CheckLowStockAlertsAsync(CancellationToken.None),
            cronExpression: "0 */6 * * *",
            options: new RecurringJobOptions { TimeZone = Manila });

        // Daily midnight UTC (8 AM PHT) — flag overdue equipment as NeedsMaintenance.
        manager.AddOrUpdate<InventoryJobService>(
            recurringJobId: "inventory-equipment-maintenance",
            methodCall: job => job.CheckEquipmentMaintenanceAsync(CancellationToken.None),
            cronExpression: Cron.Daily(hour: 0),
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

        // ── Booking (Customer Connect) ───────────────────────────────────────

        // Every 5 minutes — auto-enqueue Confirmed/Arrived bookings whose slot
        // starts within the next 15 minutes and have no queue entry yet.
        manager.AddOrUpdate<BookingJobService>(
            recurringJobId: "booking-create-queue",
            methodCall: job => job.CreateQueueFromBookingsAsync(CancellationToken.None),
            cronExpression: "*/5 * * * *",
            options: new RecurringJobOptions { TimeZone = Manila });

        // Every 5 minutes — flip still-Confirmed bookings to NoShow once the
        // slot's end + per-branch grace window has elapsed.
        manager.AddOrUpdate<BookingJobService>(
            recurringJobId: "booking-noshow-sweep",
            methodCall: job => job.MarkBookingNoShowsAsync(CancellationToken.None),
            cronExpression: "*/5 * * * *",
            options: new RecurringJobOptions { TimeZone = Manila });

        // Every hour — fire 2-hour pre-slot reminder SMS to customers with
        // Confirmed bookings. Counts against tenant SMS monthly quota.
        manager.AddOrUpdate<BookingJobService>(
            recurringJobId: "booking-reminder",
            methodCall: job => job.SendBookingReminderAsync(CancellationToken.None),
            cronExpression: Cron.Hourly(),
            options: new RecurringJobOptions { TimeZone = Manila });

        // ── Referrals ────────────────────────────────────────────────────────

        // Daily 01:00 PHT — expire Pending referral codes older than 90 days
        // that were never redeemed (no ReferredCustomerId).
        manager.AddOrUpdate<ReferralJobService>(
            recurringJobId: "referral-expiry",
            methodCall: job => job.ExpireReferralsAsync(CancellationToken.None),
            cronExpression: Cron.Daily(hour: 1),
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

        // ── Expenses ─────────────────────────────────────────────────────────

        // Daily 00:30 PHT — auto-generate records for recurring expenses.
        manager.AddOrUpdate<ExpenseJobService>(
            recurringJobId: "expense-recurring-generation",
            methodCall: job => job.GenerateRecurringExpensesAsync(CancellationToken.None),
            cronExpression: Cron.Daily(hour: 0, minute: 30),
            options: new RecurringJobOptions { TimeZone = Manila });

        // ── Franchise ────────────────────────────────────────────────────────

        // 1st of each month 02:00 PHT — calculate royalties for previous month.
        manager.AddOrUpdate<FranchiseJobService>(
            recurringJobId: "franchise-monthly-royalties",
            methodCall: job => job.CalculateMonthlyRoyaltiesAsync(CancellationToken.None),
            cronExpression: "0 2 1 * *",
            options: new RecurringJobOptions { TimeZone = Manila });

        // 5th of each month 09:00 PHT — mark overdue royalties and send reminders.
        manager.AddOrUpdate<FranchiseJobService>(
            recurringJobId: "franchise-royalty-reminders",
            methodCall: job => job.SendRoyaltyRemindersAsync(CancellationToken.None),
            cronExpression: "0 9 5 * *",
            options: new RecurringJobOptions { TimeZone = Manila });

        return app;
    }
}
