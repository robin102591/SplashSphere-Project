using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Persists an in-app notification each time the hourly job fires a 2-hour
/// pre-slot reminder SMS. Keeps an audit trail for operators.
/// </summary>
public sealed class BookingReminderSentNotificationHandler(
    INotificationService notifications)
    : INotificationHandler<DomainEventNotification<BookingReminderSentEvent>>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task Handle(
        DomainEventNotification<BookingReminderSentEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;
        var slotLocal = (e.SlotStartUtc + ManilaOffset).ToString("h:mm tt");

        await notifications.SendAsync(new SendNotificationRequest
        {
            TenantId = e.TenantId,
            Type = NotificationType.BookingReminderSent,
            Title = "Booking reminder sent",
            Message = $"Reminder SMS sent for plate {e.PlateNumber} — slot at {slotLocal}.",
            ReferenceId = e.BookingId,
            ReferenceType = "Booking",
        }, cancellationToken);
    }
}
