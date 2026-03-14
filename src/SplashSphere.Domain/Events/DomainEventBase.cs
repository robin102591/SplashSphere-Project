namespace SplashSphere.Domain.Events;

/// <summary>
/// Base record for all domain events. Provides default implementations of
/// <see cref="IDomainEvent.EventId"/> and <see cref="IDomainEvent.OccurredAt"/>
/// so derived records only need to declare their payload properties.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    /// <inheritdoc/>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <inheritdoc/>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
