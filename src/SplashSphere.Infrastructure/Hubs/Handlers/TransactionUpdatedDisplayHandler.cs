using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Sends <c>DisplayTransactionUpdated</c> when an in-progress transaction's
/// line items change (services / packages / merchandise). Fires off
/// <see cref="TransactionUpdatedEvent"/> raised by
/// <c>UpdateTransactionItemsCommandHandler</c>.
/// </summary>
public sealed class TransactionUpdatedDisplayHandler(
    IDisplayBroadcaster broadcaster)
    : INotificationHandler<DomainEventNotification<TransactionUpdatedEvent>>
{
    public Task Handle(
        DomainEventNotification<TransactionUpdatedEvent> notification,
        CancellationToken cancellationToken)
        => broadcaster.BroadcastUpdatedAsync(notification.Event.TransactionId, cancellationToken);
}
