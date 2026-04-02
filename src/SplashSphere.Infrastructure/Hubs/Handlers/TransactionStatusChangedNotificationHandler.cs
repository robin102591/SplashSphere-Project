using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Handles <see cref="TransactionStatusChangedEvent"/> and broadcasts:
/// <list type="bullet">
///   <item><c>TransactionUpdated</c> → branch group — status change visible on POS.</item>
///   <item><c>DashboardMetricsUpdated</c> → tenant group — only when transition is to
///   <see cref="TransactionStatus.Completed"/> (revenue KPI changed).</item>
/// </list>
/// Fetches the current <c>FinalAmount</c> and <c>TransactionNumber</c> from the
/// database since <see cref="TransactionStatusChangedEvent"/> does not carry them.
/// </summary>
public sealed class TransactionStatusChangedNotificationHandler(
    IHubContext<SplashSphereHub> hub,
    IApplicationDbContext db,
    INotificationService notificationService,
    ILogger<TransactionStatusChangedNotificationHandler> logger)
    : INotificationHandler<DomainEventNotification<TransactionStatusChangedEvent>>
{
    public async Task Handle(
        DomainEventNotification<TransactionStatusChangedEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        // Fetch display fields not carried on the event.
        var tx = await db.Transactions
            .AsNoTracking()
            .Where(t => t.Id == e.TransactionId)
            .Select(t => new { t.TransactionNumber, t.FinalAmount })
            .FirstOrDefaultAsync(cancellationToken);

        await hub.Clients
            .Group(SplashSphereHub.BranchGroup(e.TenantId, e.BranchId))
            .SendAsync("TransactionUpdated", new TransactionUpdatedPayload(
                e.TransactionId,
                e.BranchId,
                tx?.TransactionNumber ?? string.Empty,
                e.NewStatus.ToString(),
                tx?.FinalAmount ?? 0m),
                cancellationToken);

        // Revenue KPI only changes when a transaction completes.
        if (e.NewStatus == TransactionStatus.Completed)
        {
            await hub.Clients
                .Group(SplashSphereHub.TenantGroup(e.TenantId))
                .SendAsync("DashboardMetricsUpdated", new DashboardMetricsUpdatedPayload(
                    e.TenantId,
                    e.BranchId),
                    cancellationToken);

            // Persist a notification for completed transactions.
            await notificationService.CreateAsync(
                e.TenantId,
                Domain.Enums.NotificationType.TransactionCompleted,
                Domain.Enums.NotificationCategory.Operations,
                "Transaction Completed",
                $"Transaction {tx?.TransactionNumber ?? e.TransactionId} completed — ₱{tx?.FinalAmount ?? 0:N2}",
                e.TransactionId,
                "Transaction",
                cancellationToken);

            // Propagate completion to the queue board and wall TV display.
            if (e.QueueEntryId is not null)
            {
                var qe = await db.QueueEntries
                    .AsNoTracking()
                    .Where(q => q.Id == e.QueueEntryId)
                    .Select(q => new { q.QueueNumber, q.PlateNumber })
                    .FirstOrDefaultAsync(cancellationToken);

                if (qe is not null)
                {
                    await hub.Clients
                        .Group(SplashSphereHub.BranchGroup(e.TenantId, e.BranchId))
                        .SendAsync("QueueUpdated", new QueueUpdatedPayload(
                            e.QueueEntryId,
                            e.BranchId,
                            qe.QueueNumber,
                            qe.PlateNumber,
                            "Completed",
                            Priority: string.Empty,
                            EstimatedWaitMinutes: null),
                            cancellationToken);

                    _ = BuildAndSendDisplaySnapshotAsync(e.BranchId);
                }
            }
        }
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
