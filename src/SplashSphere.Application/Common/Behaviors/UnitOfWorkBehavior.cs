using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Common.Behaviors;

/// <summary>
/// Innermost pipeline behavior. After the command handler returns successfully,
/// flushes all EF Core tracked changes via <see cref="IUnitOfWork.SaveChangesAsync"/>.
/// <para>
/// Skipped entirely for queries (any <see cref="IRequest{TResponse}"/> that does NOT
/// implement <see cref="IBaseCommand"/>), so read paths never incur a redundant
/// <c>SaveChanges</c> call.
/// </para>
/// <para>
/// Also skipped when the handler returns a <see cref="Result"/> failure — the handler
/// signalled a domain error and partial state should not be persisted.
/// </para>
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Pass straight through for queries — IBaseCommand not implemented.
        if (request is not IBaseCommand)
            return await next(cancellationToken);

        var response = await next(cancellationToken);

        // Handler returned a domain failure — do not persist partial state.
        if (response is Result { IsFailure: true })
            return response;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
