using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Sends <c>DisplayTransactionStarted</c> when a transaction is created so
/// the paired customer display transitions Idle → Building. The broadcaster
/// is a no-op for transactions without a <c>PosStationId</c>, so admin-app
/// transactions and legacy transactions cost nothing.
/// </summary>
public sealed class TransactionCreatedDisplayHandler(
    IDisplayBroadcaster broadcaster)
    : INotificationHandler<DomainEventNotification<TransactionCreatedEvent>>
{
    public Task Handle(
        DomainEventNotification<TransactionCreatedEvent> notification,
        CancellationToken cancellationToken)
        => broadcaster.BroadcastStartedAsync(notification.Event.TransactionId, cancellationToken);
}
