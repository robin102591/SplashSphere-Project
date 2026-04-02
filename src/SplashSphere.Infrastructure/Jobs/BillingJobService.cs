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
    ILogger<BillingJobService> logger,
    IEmailService emailService)
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

        // Load tenant emails for sending reminders
        var tenantIds = expiring.Select(e => e.TenantId).ToList();
        var tenantEmails = await db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => tenantIds.Contains(t.Id))
            .Select(t => new { t.Id, t.Email, t.Name })
            .ToDictionaryAsync(t => t.Id, ct);

        foreach (var sub in expiring)
        {
            var daysLeft = (int)(sub.TrialEndDate - now).TotalDays;
            logger.LogInformation(
                "BillingJob: Trial expiry reminder for tenant {TenantId} — {Days} day(s) remaining.",
                sub.TenantId, daysLeft);

            if (tenantEmails.TryGetValue(sub.TenantId, out var tenant) && !string.IsNullOrEmpty(tenant.Email))
            {
                await emailService.SendAsync(new EmailMessage(
                    To: tenant.Email,
                    Subject: $"Your SplashSphere trial ends in {daysLeft} day{(daysLeft != 1 ? "s" : "")}",
                    HtmlBody: $"""
                        <h2>Hi {tenant.Name},</h2>
                        <p>Your SplashSphere free trial expires in <strong>{daysLeft} day{(daysLeft != 1 ? "s" : "")}</strong>.</p>
                        <p>Upgrade now to keep all your data and continue using SplashSphere without interruption.</p>
                        <p>If you have any questions, just reply to this email — we're happy to help.</p>
                        <p>— The SplashSphere Team</p>
                        """), ct);
            }
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

        // Load tenant info for invoice emails
        var allTenantIds = activeSubscriptions.Select(s => s.TenantId).ToList();
        var tenantInfo = await db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => allTenantIds.Contains(t.Id))
            .Select(t => new { t.Id, t.Email, t.Name })
            .ToDictionaryAsync(t => t.Id, ct);

        var invoicesToEmail = new List<(string TenantId, string InvoiceNumber, decimal Amount)>();

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
            invoicesToEmail.Add((sub.TenantId, invoiceNumber, plan.MonthlyPrice));
            created++;
            sequence++;
        }

        if (created > 0)
            await db.SaveChangesAsync(ct);

        // Send invoice emails after successful save
        foreach (var (tenantId, invoiceNumber, amount) in invoicesToEmail)
        {
            if (tenantInfo.TryGetValue(tenantId, out var tenant) && !string.IsNullOrEmpty(tenant.Email))
            {
                await emailService.SendAsync(new EmailMessage(
                    To: tenant.Email,
                    Subject: $"SplashSphere Invoice {invoiceNumber}",
                    HtmlBody: $"""
                        <h2>Hi {tenant.Name},</h2>
                        <p>Your monthly SplashSphere invoice is ready.</p>
                        <p><strong>Invoice:</strong> {invoiceNumber}<br/>
                        <strong>Amount:</strong> ₱{amount:N2}<br/>
                        <strong>Due:</strong> {DateTime.UtcNow.AddDays(7):MMMM d, yyyy}</p>
                        <p>Log in to your dashboard to view details or make a payment.</p>
                        <p>— The SplashSphere Team</p>
                        """), ct);
            }
        }

        logger.LogInformation(
            "BillingJob: Generated {Count} invoice(s) for {Month}.", created, invoiceMonth);
    }
}
