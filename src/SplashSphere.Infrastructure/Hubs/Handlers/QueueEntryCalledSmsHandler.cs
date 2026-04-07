using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Events;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Sends an SMS to the customer when their vehicle is called from the queue.
/// Checks: SMS feature enabled, budget remaining, customer has phone number.
/// Increments the tenant's SMS usage counter on success.
/// </summary>
public sealed class QueueEntryCalledSmsHandler(
    ISmsService smsService,
    IPlanEnforcementService planService,
    IApplicationDbContext db,
    ILogger<QueueEntryCalledSmsHandler> logger)
    : INotificationHandler<DomainEventNotification<QueueEntryCalledEvent>>
{
    public async Task Handle(
        DomainEventNotification<QueueEntryCalledEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        if (!await planService.HasFeatureAsync(e.TenantId, FeatureKeys.SmsNotifications, cancellationToken))
            return;

        var remaining = await planService.GetSmsBudgetRemainingAsync(e.TenantId, cancellationToken);
        if (remaining <= 0)
        {
            logger.LogDebug("SMS budget exhausted for tenant {TenantId}, skipping.", e.TenantId);
            return;
        }

        // Load customer phone from the queue entry
        var customer = await db.QueueEntries
            .AsNoTracking()
            .Where(q => q.Id == e.QueueEntryId && q.CustomerId != null)
            .Select(q => new { q.Customer!.ContactNumber, q.Customer.FirstName })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null || string.IsNullOrEmpty(customer.ContactNumber))
            return;

        var body = $"Hi {customer.FirstName}! Your vehicle ({e.PlateNumber}) " +
                   $"is now being called (Queue #{e.QueueNumber}). " +
                   $"Please proceed to the service bay.";

        var sent = await smsService.SendAsync(new SmsMessage(customer.ContactNumber, body), cancellationToken);

        if (sent)
        {
            var sub = await db.TenantSubscriptions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.TenantId == e.TenantId, cancellationToken);

            if (sub is not null)
            {
                sub.SmsUsedThisMonth++;
                // No SaveChangesAsync here — EventPublisher.FlushAsync persists after all handlers.
                planService.EvictCache(e.TenantId);
            }
        }
    }
}
