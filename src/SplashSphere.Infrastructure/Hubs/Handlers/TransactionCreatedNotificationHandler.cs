using MediatR;
using Microsoft.AspNetCore.SignalR;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Handles <see cref="TransactionCreatedEvent"/> and broadcasts:
/// <list type="bullet">
///   <item><c>TransactionUpdated</c> → branch group — notifies POS clients a new transaction exists.</item>
///   <item><c>DashboardMetricsUpdated</c> → tenant group — transaction count KPI has changed.</item>
/// </list>
/// </summary>
public sealed class TransactionCreatedNotificationHandler(
    IHubContext<SplashSphereHub> hub)
    : INotificationHandler<DomainEventNotification<TransactionCreatedEvent>>
{
    public async Task Handle(
        DomainEventNotification<TransactionCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var e = notification.Event;

        await hub.Clients
            .Group(SplashSphereHub.BranchGroup(e.TenantId, e.BranchId))
            .SendAsync("TransactionUpdated", new TransactionUpdatedPayload(
                e.TransactionId,
                e.BranchId,
                e.TransactionNumber,
                e.Status.ToString(),
                e.FinalAmount),
                cancellationToken);

        await hub.Clients
            .Group(SplashSphereHub.TenantGroup(e.TenantId))
            .SendAsync("DashboardMetricsUpdated", new DashboardMetricsUpdatedPayload(
                e.TenantId,
                e.BranchId),
                cancellationToken);
    }
}
