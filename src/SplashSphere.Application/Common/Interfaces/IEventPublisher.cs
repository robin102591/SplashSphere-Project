using SplashSphere.SharedKernel.Abstractions;

namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Queues and publishes domain events to registered in-process handlers.
/// <para>
/// Use <see cref="Enqueue"/> inside command handlers so events are only
/// dispatched <em>after</em> <c>SaveChangesAsync</c> commits — ensuring
/// SignalR notification handlers always read consistent, already-saved data.
/// <c>UnitOfWorkBehavior</c> calls <see cref="FlushAsync"/> after the save.
/// </para>
/// </summary>
public interface IEventPublisher
{
    /// <summary>Adds the event to the pending queue. Does not publish immediately.</summary>
    void Enqueue(IDomainEvent domainEvent);

    /// <summary>Publishes all queued events in order. Clears the queue.</summary>
    Task FlushAsync(CancellationToken cancellationToken = default);
}
