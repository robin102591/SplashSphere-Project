using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Common.Behaviors;

/// <summary>
/// Innermost pipeline behavior. After the command handler returns successfully:
/// <list type="number">
///   <item>Flushes EF Core tracked changes via <see cref="IUnitOfWork.SaveChangesAsync"/>.</item>
///   <item>Publishes queued domain events via <see cref="IEventPublisher.FlushAsync"/>.</item>
/// </list>
/// Events are published <em>after</em> the save so notification handlers (e.g. SignalR
/// broadcasters) always query consistent, already-committed data.
/// <para>
/// Skipped entirely for queries (any <see cref="IRequest{TResponse}"/> that does NOT
/// implement <see cref="IBaseCommand"/>), so read paths never incur a redundant save.
/// </para>
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IBaseCommand)
            return await next(cancellationToken);

        var response = await next(cancellationToken);

        if (response is Result { IsFailure: true })
            return response;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await eventPublisher.FlushAsync(cancellationToken);

        return response;
    }
}
