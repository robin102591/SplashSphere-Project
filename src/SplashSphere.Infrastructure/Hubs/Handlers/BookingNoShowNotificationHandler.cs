using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Posts a branch notification when the no-show sweep flips a booking to
/// <see cref="BookingStatus.NoShow"/>.
/// </summary>
public sealed class BookingNoShowNotificationHandler(
    INotificationService notifications)
    : INotificationHandler<DomainEventNotification<BookingNoShowEvent>>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task Handle(
        DomainEventNotification<BookingNoShowEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;
        var slotLocal = (e.SlotStartUtc + ManilaOffset).ToString("h:mm tt");

        await notifications.SendAsync(new SendNotificationRequest
        {
            TenantId = e.TenantId,
            Type = NotificationType.BookingNoShow,
            Title = "Booking no-show",
            Message = $"Booking for plate {e.PlateNumber} marked as no-show (slot was {slotLocal}).",
            ReferenceId = e.BookingId,
            ReferenceType = "Booking",
            ActionUrl = $"/dashboard/bookings/{e.BookingId}",
            ActionLabel = "Review booking",
        }, cancellationToken);
    }
}
