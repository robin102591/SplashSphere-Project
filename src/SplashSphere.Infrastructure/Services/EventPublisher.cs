using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Abstractions;

namespace SplashSphere.Infrastructure.Services;

/// <summary>
/// Queues domain events and flushes them via MediatR after <c>SaveChangesAsync</c>.
/// Events are wrapped in <see cref="DomainEventNotification{TEvent}"/> so Domain
/// stays free of MediatR references.
/// </summary>
public sealed class EventPublisher(IPublisher publisher) : IEventPublisher
{
    private readonly List<IDomainEvent> _pending = [];

    public void Enqueue(IDomainEvent domainEvent) => _pending.Add(domainEvent);

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        // Copy and clear before publishing so re-entrant Enqueue calls in handlers
        // are collected for the next flush rather than processed in this batch.
        var batch = _pending.ToList();
        _pending.Clear();

        foreach (var domainEvent in batch)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
            await publisher.Publish(notification, cancellationToken);
        }
    }
}

/// <summary>
/// MediatR notification wrapper for a domain event. Handlers implement
/// <c>INotificationHandler&lt;DomainEventNotification&lt;TEvent&gt;&gt;</c>.
/// </summary>
public sealed record DomainEventNotification<TEvent>(TEvent Event) : INotification
    where TEvent : IDomainEvent;
