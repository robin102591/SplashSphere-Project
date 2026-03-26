using MediatR;
using Microsoft.AspNetCore.SignalR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Handles <see cref="QueueEntryNoShowEvent"/> and broadcasts:
/// <list type="bullet">
///   <item><c>QueueUpdated</c> → branch group — queue board removes/greys out the entry.</item>
///   <item><c>QueueDisplayUpdated</c> → public display group — wall TV removes the no-show from "Now Calling".</item>
/// </list>
/// </summary>
public sealed class QueueEntryNoShowNotificationHandler(
    IHubContext<SplashSphereHub> hub,
    IApplicationDbContext db,
    INotificationService notificationService)
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

        // Remove the no-show from the public display "Now Calling" section
        var snapshot = await QueueDisplaySnapshotBuilder.BuildAsync(db, e.BranchId, cancellationToken);
        await hub.Clients
            .Group(SplashSphereHub.QueueDisplayGroup(e.BranchId))
            .SendAsync("QueueDisplayUpdated", snapshot, cancellationToken);

        // Persist notification for no-show events.
        await notificationService.CreateAsync(
            e.TenantId,
            Domain.Enums.NotificationType.QueueNoShow,
            Domain.Enums.NotificationCategory.Queue,
            "Queue No-Show",
            $"Queue {e.QueueNumber} ({e.PlateNumber}) did not respond when called.",
            e.QueueEntryId,
            "QueueEntry",
            cancellationToken);
    }
}
