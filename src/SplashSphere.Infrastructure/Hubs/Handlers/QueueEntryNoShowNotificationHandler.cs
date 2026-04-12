using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Handles <see cref="QueueEntryNoShowEvent"/> and broadcasts:
/// <list type="bullet">
///   <item><c>QueueUpdated</c> → branch group — queue board removes/greys out the entry.</item>
///   <item><c>QueueDisplayUpdated</c> → public display group — wall TV removes the no-show from "Now Calling" (non-blocking).</item>
/// </list>
/// </summary>
public sealed class QueueEntryNoShowNotificationHandler(
    IHubContext<SplashSphereHub> hub,
    IApplicationDbContext db,
    INotificationService notificationService,
    ILogger<QueueEntryNoShowNotificationHandler> logger)
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

        // Build + send public display snapshot
        await BuildAndSendDisplaySnapshotAsync(e.BranchId);

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

    private async Task BuildAndSendDisplaySnapshotAsync(string branchId)
    {
        try
        {
            var snapshot = await QueueDisplaySnapshotBuilder.BuildAsync(db, branchId, CancellationToken.None);
            await hub.Clients
                .Group(SplashSphereHub.QueueDisplayGroup(branchId))
                .SendAsync("QueueDisplayUpdated", snapshot, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send queue display snapshot for branch {BranchId}.", branchId);
        }
    }
}
