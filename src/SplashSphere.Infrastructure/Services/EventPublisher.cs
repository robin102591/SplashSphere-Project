using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Abstractions;

namespace SplashSphere.Infrastructure.Services;

/// <summary>
/// Wraps each domain event in a <see cref="DomainEventNotification{TEvent}"/> and
/// dispatches it via MediatR's <see cref="IPublisher"/>. This keeps domain event records
/// free of MediatR references while still routing them through the MediatR pipeline.
/// </summary>
public sealed class EventPublisher(IPublisher publisher) : IEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
        return publisher.Publish(notification, cancellationToken);
    }
}

/// <summary>
/// MediatR notification wrapper for a domain event. Handlers implement
/// <c>INotificationHandler&lt;DomainEventNotification&lt;TEvent&gt;&gt;</c>.
/// </summary>
public sealed record DomainEventNotification<TEvent>(TEvent Event) : INotification
    where TEvent : IDomainEvent;
