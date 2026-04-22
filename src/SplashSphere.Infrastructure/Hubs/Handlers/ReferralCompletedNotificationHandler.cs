using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Posts a notification when a referral reaches the Completed state. Includes both
/// participants' names so admins can see who just earned what in the feed.
/// </summary>
public sealed class ReferralCompletedNotificationHandler(
    INotificationService notifications,
    IApplicationDbContext db)
    : INotificationHandler<DomainEventNotification<ReferralCompletedEvent>>
{
    public async Task Handle(
        DomainEventNotification<ReferralCompletedEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        var names = await (
            from referrer in db.Customers.IgnoreQueryFilters()
            from referred in db.Customers.IgnoreQueryFilters()
                .Where(c => c.Id == e.ReferredCustomerId).DefaultIfEmpty()
            where referrer.Id == e.ReferrerCustomerId
            select new
            {
                ReferrerFirst = referrer.FirstName,
                ReferredFirst = referred != null ? referred.FirstName : null,
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        var referrerName = names?.ReferrerFirst ?? "referrer";
        var referredName = names?.ReferredFirst ?? "referred customer";

        await notifications.SendAsync(new SendNotificationRequest
        {
            TenantId = e.TenantId,
            Type = NotificationType.ReferralCompleted,
            Title = "Referral completed",
            Message = $"{referrerName} earned {e.ReferrerPointsAwarded} pts — " +
                      $"{referredName} received {e.ReferredPointsAwarded} pts (code {e.ReferralCode}).",
            ReferenceId = e.ReferralId,
            ReferenceType = "Referral",
        }, cancellationToken);
    }
}
