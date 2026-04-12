using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Handles <see cref="QueueEntryCalledEvent"/> and broadcasts:
/// <list type="bullet">
///   <item><c>QueueUpdated</c> → branch group — queue board refreshes to show entry as Called.</item>
///   <item><c>QueueDisplayUpdated</c> → public display group — wall TV receives full snapshot (non-blocking).</item>
/// </list>
/// </summary>
public sealed class QueueEntryCalledNotificationHandler(
    IHubContext<SplashSphereHub> hub,
    IApplicationDbContext db,
    ILogger<QueueEntryCalledNotificationHandler> logger)
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

        // Build + send public display snapshot
        await BuildAndSendDisplaySnapshotAsync(e.BranchId);
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
