using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
    IApplicationDbContext db)
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
        }
    }
}
