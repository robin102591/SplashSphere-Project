using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Posts a branch notification when a new online booking is confirmed.
/// Message format: "New booking at 9:00 AM — Toyota Vios (ABC 1234)".
/// </summary>
public sealed class BookingConfirmedNotificationHandler(
    INotificationService notifications,
    IApplicationDbContext db)
    : INotificationHandler<DomainEventNotification<BookingConfirmedEvent>>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task Handle(
        DomainEventNotification<BookingConfirmedEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        // Try to look up make/model for a friendlier label; fall back to plate.
        var vehicleLabel = await (
            from b in db.Bookings.IgnoreQueryFilters()
            join v in db.ConnectVehicles.IgnoreQueryFilters() on b.ConnectVehicleId equals v.Id
            join make in db.GlobalMakes.IgnoreQueryFilters() on v.MakeId equals make.Id
            join model in db.GlobalModels.IgnoreQueryFilters() on v.ModelId equals model.Id
            where b.Id == e.BookingId
            select make.Name + " " + model.Name)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        var slotLocal = (e.SlotStartUtc + ManilaOffset).ToString("h:mm tt");
        var vehicle = string.IsNullOrWhiteSpace(vehicleLabel)
            ? e.PlateNumber
            : $"{vehicleLabel} ({e.PlateNumber})";

        await notifications.SendAsync(new SendNotificationRequest
        {
            TenantId = e.TenantId,
            Type = NotificationType.BookingConfirmed,
            Title = "New booking",
            Message = $"New booking at {slotLocal} — {vehicle}.",
            ReferenceId = e.BookingId,
            ReferenceType = "Booking",
            ActionUrl = $"/dashboard/bookings/{e.BookingId}",
            ActionLabel = "View booking",
        }, cancellationToken);
    }
}
