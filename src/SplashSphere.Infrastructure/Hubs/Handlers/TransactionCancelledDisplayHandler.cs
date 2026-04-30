using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Sends <c>DisplayTransactionCancelled</c> when a transaction transitions to
/// the <see cref="TransactionStatus.Cancelled"/> terminal state. The display
/// returns to Idle on receipt — it doesn't show the cancellation explicitly
/// to the customer (no need to surface internal POS workflow).
/// <para>
/// We listen on <see cref="TransactionStatusChangedEvent"/> rather than
/// adding a dedicated event because cancellation is the only non-completion
/// terminal transition and adding another event for one consumer isn't
/// worth the noise.
/// </para>
/// </summary>
public sealed class TransactionCancelledDisplayHandler(
    IDisplayBroadcaster broadcaster)
    : INotificationHandler<DomainEventNotification<TransactionStatusChangedEvent>>
{
    public Task Handle(
        DomainEventNotification<TransactionStatusChangedEvent> notification,
        CancellationToken cancellationToken)
    {
        if (notification.Event.NewStatus != TransactionStatus.Cancelled)
            return Task.CompletedTask;

        return broadcaster.BroadcastCancelledAsync(
            notification.Event.TransactionId,
            cancellationToken);
    }
}
