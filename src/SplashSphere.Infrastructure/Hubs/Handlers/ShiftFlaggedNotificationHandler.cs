using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Handles <see cref="ShiftFlaggedEvent"/> raised when a manager flags a shift
/// for investigation. Persists a <c>Notification</c> record so admins who are
/// offline can see the flagged shift.
/// </summary>
public sealed class ShiftFlaggedNotificationHandler(
    INotificationService notificationService)
    : INotificationHandler<DomainEventNotification<ShiftFlaggedEvent>>
{
    public async Task Handle(
        DomainEventNotification<ShiftFlaggedEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        await notificationService.CreateAsync(
            e.TenantId,
            NotificationType.ShiftFlagged,
            NotificationCategory.Finance,
            "Shift Flagged",
            $"Shift flagged for investigation — variance ₱{e.Variance:N2}. Notes: {e.Notes}",
            e.ShiftId,
            "Shift",
            cancellationToken);
    }
}
