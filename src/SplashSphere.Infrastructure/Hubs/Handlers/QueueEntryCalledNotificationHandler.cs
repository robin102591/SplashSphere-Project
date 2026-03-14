using MediatR;
using Microsoft.AspNetCore.SignalR;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Handles <see cref="QueueEntryCalledEvent"/> and broadcasts:
/// <list type="bullet">
///   <item><c>QueueUpdated</c> → branch group — queue board shows entry as Called.</item>
///   <item><c>QueueDisplayUpdated</c> → public display group — wall TV highlights the called number.</item>
/// </list>
/// </summary>
public sealed class QueueEntryCalledNotificationHandler(
    IHubContext<SplashSphereHub> hub)
    : INotificationHandler<DomainEventNotification<QueueEntryCalledEvent>>
{
    public async Task Handle(
        DomainEventNotification<QueueEntryCalledEvent> notification,
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
                "Called",
                Priority: string.Empty,        // not carried on this event; client uses existing state
                EstimatedWaitMinutes: null),
                cancellationToken);

        await hub.Clients
            .Group(SplashSphereHub.QueueDisplayGroup(e.BranchId))
            .SendAsync("QueueDisplayUpdated", new QueueDisplayUpdatedPayload(
                e.QueueEntryId,
                e.BranchId,
                e.QueueNumber,
                MaskPlate(e.PlateNumber),
                "Called",
                Priority: string.Empty,
                EstimatedWaitMinutes: null),
                cancellationToken);
    }

    private static string MaskPlate(string plate) =>
        plate.Length <= 3
            ? plate
            : $"{plate[..2]}{new string('*', plate.Length - 3)}{plate[^1]}";
}
