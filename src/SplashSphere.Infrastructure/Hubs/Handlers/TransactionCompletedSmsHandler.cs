using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Events;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Sends an SMS to the customer when their transaction is completed.
/// Checks: SMS feature enabled, budget remaining, customer has phone number.
/// Increments the tenant's SMS usage counter on success.
/// </summary>
public sealed class TransactionCompletedSmsHandler(
    ISmsService smsService,
    IPlanEnforcementService planService,
    IApplicationDbContext db,
    ILogger<TransactionCompletedSmsHandler> logger)
    : INotificationHandler<DomainEventNotification<TransactionCompletedEvent>>
{
    public async Task Handle(
        DomainEventNotification<TransactionCompletedEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        // Check if tenant has SMS feature enabled
        if (!await planService.HasFeatureAsync(e.TenantId, FeatureKeys.SmsNotifications, cancellationToken))
            return;

        // Check SMS budget
        var remaining = await planService.GetSmsBudgetRemainingAsync(e.TenantId, cancellationToken);
        if (remaining <= 0)
        {
            logger.LogDebug("SMS budget exhausted for tenant {TenantId}, skipping.", e.TenantId);
            return;
        }

        // Load customer phone from the transaction
        var customer = await db.Transactions
            .AsNoTracking()
            .Where(t => t.Id == e.TransactionId && t.CustomerId != null)
            .Select(t => new { t.Customer!.ContactNumber, t.Customer.FirstName })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null || string.IsNullOrEmpty(customer.ContactNumber))
            return;

        var body = $"Hi {customer.FirstName}! Your car wash is complete. " +
                   $"Total: P{e.FinalAmount:N2}. " +
                   $"Ref: {e.TransactionNumber}. Thank you for choosing SplashSphere!";

        var sent = await smsService.SendAsync(new SmsMessage(customer.ContactNumber, body), cancellationToken);

        if (sent)
        {
            // Increment SMS usage counter
            var sub = await db.TenantSubscriptions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.TenantId == e.TenantId, cancellationToken);

            if (sub is not null)
            {
                sub.SmsUsedThisMonth++;
                await db.SaveChangesAsync(cancellationToken);
                planService.EvictCache(e.TenantId);
            }
        }
    }
}
