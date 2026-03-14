using SplashSphere.SharedKernel.Abstractions;

namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Publishes domain events to registered in-process handlers.
/// Implemented in Infrastructure using MediatR's <c>IPublisher</c> with a
/// <c>DomainEventNotification&lt;T&gt;</c> wrapper so the Domain layer stays
/// free of MediatR references.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
