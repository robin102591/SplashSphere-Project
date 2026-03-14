namespace SplashSphere.SharedKernel.Abstractions;

/// <summary>
/// Marker interface for domain events. Kept free of framework dependencies
/// so SharedKernel stays a pure library. The Application layer wraps these
/// in MediatR <c>INotification</c> envelopes before publishing.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Unique identifier for this event occurrence.</summary>
    Guid EventId { get; }

    /// <summary>UTC timestamp when the event was raised.</summary>
    DateTime OccurredAt { get; }
}
