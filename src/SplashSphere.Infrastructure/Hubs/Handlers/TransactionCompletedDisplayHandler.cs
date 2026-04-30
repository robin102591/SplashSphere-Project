using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Sends <c>DisplayTransactionCompleted</c> when a transaction transitions to
/// Completed. Drives the display's "Payment Complete" screen which
/// auto-reverts to Idle after the configured hold duration.
/// </summary>
public sealed class TransactionCompletedDisplayHandler(
    IDisplayBroadcaster broadcaster)
    : INotificationHandler<DomainEventNotification<TransactionCompletedEvent>>
{
    public Task Handle(
        DomainEventNotification<TransactionCompletedEvent> notification,
        CancellationToken cancellationToken)
        => broadcaster.BroadcastCompletedAsync(notification.Event.TransactionId, cancellationToken);
}
