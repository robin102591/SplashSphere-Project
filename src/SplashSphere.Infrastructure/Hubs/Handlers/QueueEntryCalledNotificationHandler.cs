using MediatR;
using Microsoft.AspNetCore.SignalR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Handles <see cref="QueueEntryCalledEvent"/> and broadcasts:
/// <list type="bullet">
///   <item><c>QueueUpdated</c> → branch group — queue board refreshes to show entry as Called.</item>
///   <item><c>QueueDisplayUpdated</c> → public display group — wall TV receives full snapshot.</item>
/// </list>
/// </summary>
public sealed class QueueEntryCalledNotificationHandler(
    IHubContext<SplashSphereHub> hub,
    IApplicationDbContext db)
    : INotificationHandler<DomainEventNotification<QueueEntryCalledEvent>>
{
    public async Task Handle(
        DomainEventNotification<QueueEntryCalledEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        // Notify the POS queue board (branch group — requires auth)
        await hub.Clients
            .Group(SplashSphereHub.BranchGroup(e.TenantId, e.BranchId))
            .SendAsync("QueueUpdated", new QueueUpdatedPayload(
                e.QueueEntryId,
                e.BranchId,
                e.QueueNumber,
                e.PlateNumber,
                "Called",
                Priority: string.Empty,
                EstimatedWaitMinutes: null),
                cancellationToken);

        // Send full display snapshot to public wall TV
        var snapshot = await QueueDisplaySnapshotBuilder.BuildAsync(db, e.BranchId, cancellationToken);
        await hub.Clients
            .Group(SplashSphereHub.QueueDisplayGroup(e.BranchId))
            .SendAsync("QueueDisplayUpdated", snapshot, cancellationToken);
    }
}
