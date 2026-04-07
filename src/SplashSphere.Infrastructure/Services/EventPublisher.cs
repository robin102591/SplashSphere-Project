using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Interfaces;
using SplashSphere.SharedKernel.Abstractions;

namespace SplashSphere.Infrastructure.Services;

/// <summary>
/// Queues domain events and flushes them via MediatR after <c>SaveChangesAsync</c>.
/// Events are wrapped in <see cref="DomainEventNotification{TEvent}"/> so Domain
/// stays free of MediatR references.
/// <para>
/// After publishing each event (and running all its handlers sequentially),
/// a <c>SaveChangesAsync</c> is issued to persist any changes the handlers made.
/// Then any newly-enqueued events are flushed in a subsequent loop.
/// This means notification handlers do NOT need to call <c>SaveChangesAsync</c>
/// themselves — the publisher handles it.
/// </para>
/// </summary>
public sealed class EventPublisher(IPublisher publisher, IUnitOfWork unitOfWork) : IEventPublisher
{
    private readonly List<IDomainEvent> _pending = [];

    public void Enqueue(IDomainEvent domainEvent) => _pending.Add(domainEvent);

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        // Loop until no more events are enqueued. Handlers may enqueue follow-up
        // events (e.g. TierUpgradedEvent), which are processed in the next iteration.
        const int maxIterations = 10; // safety net against infinite loops
        var iteration = 0;

        while (_pending.Count > 0 && iteration++ < maxIterations)
        {
            var batch = _pending.ToList();
            _pending.Clear();

            foreach (var domainEvent in batch)
            {
                var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
                var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
                await publisher.Publish(notification, cancellationToken);
            }

            // Persist any changes made by notification handlers in this batch.
            // This replaces individual SaveChangesAsync calls in handlers.
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

/// <summary>
/// MediatR notification wrapper for a domain event. Handlers implement
/// <c>INotificationHandler&lt;DomainEventNotification&lt;TEvent&gt;&gt;</c>.
/// </summary>
public sealed record DomainEventNotification<TEvent>(TEvent Event) : INotification
    where TEvent : IDomainEvent;
