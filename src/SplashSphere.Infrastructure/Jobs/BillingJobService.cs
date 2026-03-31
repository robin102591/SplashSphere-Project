using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Infrastructure.Jobs;

/// <summary>
/// Hangfire jobs for subscription billing lifecycle:
/// trial reminders, overdue suspension, SMS reset, and invoice generation.
/// </summary>
public sealed class BillingJobService(
    IServiceScopeFactory scopeFactory,
    ILogger<BillingJobService> logger)
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    /// <summary>
    /// Daily 9 AM PHT — send reminders to tenants whose trial ends in 1-3 days.
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task SendTrialExpiryReminderAsync(CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var now = DateTime.UtcNow;

        var expiring = await db.TenantSubscriptions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => s.Status == SubscriptionStatus.Trial &&
                        s.TrialEndDate > now &&
                        s.TrialEndDate <= now.AddDays(3))
            .Select(s => new { s.TenantId, s.TrialEndDate })
            .ToListAsync(ct);

        if (expiring.Count == 0) return;

        foreach (var sub in expiring)
        {
            var daysLeft = (int)(sub.TrialEndDate - now).TotalDays;
            logger.LogInformation(
                "BillingJob: Trial expiry reminder for tenant {TenantId} — {Days} day(s) remaining.",
                sub.TenantId, daysLeft);
            // TODO: Send email/notification via notification service
        }

        logger.LogInformation(
            "BillingJob: Sent {Count} trial expiry reminder(s).", expiring.Count);
    }

    /// <summary>
    /// Daily 9 AM PHT — suspend accounts that have been PastDue for over 7 days.
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task SuspendOverdueAccountsAsync(CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var cutoff = DateTime.UtcNow.AddDays(-7);

        var overdue = await db.TenantSubscriptions
            .IgnoreQueryFilters()
            .Where(s => s.Status == SubscriptionStatus.PastDue &&
                        s.PastDueSince != null &&
                        s.PastDueSince <= cutoff)
            .ToListAsync(ct);

        if (overdue.Count == 0) return;

        var planService = scope.ServiceProvider.GetRequiredService<IPlanEnforcementService>();

        foreach (var sub in overdue)
        {
            sub.Status = SubscriptionStatus.Suspended;
            planService.EvictCache(sub.TenantId);

            logger.LogWarning(
                "BillingJob: Suspended tenant {TenantId} — PastDue for over 7 days.",
                sub.TenantId);
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "BillingJob: Suspended {Count} overdue account(s).", overdue.Count);
    }

    /// <summary>
    /// 1st of each month — reset SMS usage counters for all subscriptions.
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task ResetMonthlySmsCountAsync(CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var updated = await db.TenantSubscriptions
            .IgnoreQueryFilters()
            .Where(s => s.SmsUsedThisMonth > 0)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.SmsUsedThisMonth, 0)
                .SetProperty(x => x.SmsCountResetDate, DateTime.UtcNow),
                ct);

        logger.LogInformation(
            "BillingJob: Reset SMS count for {Count} subscription(s).", updated);
    }

    /// <summary>
    /// 1st of each month — generate invoice records for the previous month.
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task GenerateMonthlyInvoicesAsync(CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var todayManila = DateOnly.FromDateTime(DateTime.UtcNow + ManilaOffset);
        var prevMonth = todayManila.AddMonths(-1);
        var invoiceMonth = $"{prevMonth.Year}-{prevMonth.Month:D2}";

        var activeSubscriptions = await db.TenantSubscriptions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => s.Status == SubscriptionStatus.Active)
            .Select(s => new { s.Id, s.TenantId, s.PlanTier })
            .ToListAsync(ct);

        // Check which tenants already have an invoice for this month
        var existingInvoices = await db.BillingRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(b => b.InvoiceNumber != null && b.InvoiceNumber.Contains(invoiceMonth))
            .Select(b => b.TenantId)
            .ToListAsync(ct);
        var existingInvoiceSet = existingInvoices.ToHashSet();

        var created = 0;
        var sequence = existingInvoiceSet.Count + 1;

        foreach (var sub in activeSubscriptions)
        {
            if (existingInvoiceSet.Contains(sub.TenantId))
                continue;

            var plan = Domain.Subscription.PlanCatalog.GetPlan(sub.PlanTier);
            var invoiceNumber = $"INV-{invoiceMonth}-{sequence:D4}";

            var billing = new Domain.Entities.BillingRecord(
                sub.TenantId,
                sub.Id,
                plan.MonthlyPrice,
                BillingType.Subscription,
                DateTime.UtcNow)
            {
                InvoiceNumber = invoiceNumber,
                DueDate = DateTime.UtcNow.AddDays(7),
            };

            db.BillingRecords.Add(billing);
            created++;
            sequence++;
        }

        if (created > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "BillingJob: Generated {Count} invoice(s) for {Month}.", created, invoiceMonth);
    }
}
