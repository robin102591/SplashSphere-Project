using MediatR;
using Microsoft.AspNetCore.SignalR;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Handles <see cref="QueueEntryCreatedEvent"/> and broadcasts:
/// <list type="bullet">
///   <item><c>QueueUpdated</c> → branch group — queue board refreshes with the new entry.</item>
///   <item><c>QueueDisplayUpdated</c> → public display group — wall TV shows the new arrival
///   with a masked plate number.</item>
/// </list>
/// </summary>
public sealed class QueueEntryCreatedNotificationHandler(
    IHubContext<SplashSphereHub> hub)
    : INotificationHandler<DomainEventNotification<QueueEntryCreatedEvent>>
{
    public async Task Handle(
        DomainEventNotification<QueueEntryCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        await hub.Clients
            .Group(SplashSphereHub.BranchGroup(e.TenantId, e.BranchId))
            .SendAsync("QueueUpdated", new QueueUpdatedPayload(
                e.QueueEntryId,
                e.BranchId,
                e.QueueNumber,
                e.PlateNumber,
                "Waiting",
                e.Priority.ToString(),
                e.EstimatedWaitMinutes),
                cancellationToken);

        await hub.Clients
            .Group(SplashSphereHub.QueueDisplayGroup(e.BranchId))
            .SendAsync("QueueDisplayUpdated", new QueueDisplayUpdatedPayload(
                e.QueueEntryId,
                e.BranchId,
                e.QueueNumber,
                MaskPlate(e.PlateNumber),
                "Waiting",
                e.Priority.ToString(),
                e.EstimatedWaitMinutes),
                cancellationToken);
    }

    /// <summary>
    /// Masks the middle characters of a plate number for the public display.
    /// "ABC123" → "AB***3". Plates of 3 chars or fewer are returned as-is.
    /// </summary>
    private static string MaskPlate(string plate) =>
        plate.Length <= 3
            ? plate
            : $"{plate[..2]}{new string('*', plate.Length - 3)}{plate[^1]}";
}
