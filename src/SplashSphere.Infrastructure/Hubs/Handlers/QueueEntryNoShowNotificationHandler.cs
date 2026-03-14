using MediatR;
using Microsoft.AspNetCore.SignalR;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Handles <see cref="QueueEntryNoShowEvent"/> and broadcasts:
/// <list type="bullet">
///   <item><c>QueueUpdated</c> → branch group — queue board removes/greys out the entry.</item>
/// </list>
/// The public display does not receive a <c>QueueDisplayUpdated</c> for no-shows — the entry
/// simply disappears from the Waiting/Called list when the board re-renders.
/// </summary>
public sealed class QueueEntryNoShowNotificationHandler(
    IHubContext<SplashSphereHub> hub)
    : INotificationHandler<DomainEventNotification<QueueEntryNoShowEvent>>
{
    public async Task Handle(
        DomainEventNotification<QueueEntryNoShowEvent> notification,
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
                "NoShow",
                Priority: string.Empty,
                EstimatedWaitMinutes: null),
                cancellationToken);
    }
}
