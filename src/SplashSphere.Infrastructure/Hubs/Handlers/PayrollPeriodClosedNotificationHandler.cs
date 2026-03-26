using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Handles <see cref="PayrollPeriodClosedEvent"/> raised when a payroll period
/// transitions from Open → Closed. Persists a notification so admins can review.
/// </summary>
public sealed class PayrollPeriodClosedNotificationHandler(
    INotificationService notificationService)
    : INotificationHandler<DomainEventNotification<PayrollPeriodClosedEvent>>
{
    public async Task Handle(
        DomainEventNotification<PayrollPeriodClosedEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        await notificationService.CreateAsync(
            e.TenantId,
            NotificationType.PayrollClosed,
            NotificationCategory.Finance,
            "Payroll Period Closed",
            $"Payroll period {e.StartDate:MMM d} – {e.EndDate:MMM d, yyyy} has been closed with {e.EntryCount} entries.",
            e.PayrollPeriodId,
            "PayrollPeriod",
            cancellationToken);
    }
}
