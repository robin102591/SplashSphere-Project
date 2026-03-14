using MediatR;

namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Marker for read-only queries. Handlers must not modify state.
/// Excluded from <c>UnitOfWorkBehavior</c> automatically — no <see cref="IBaseCommand"/>
/// is implemented, so <c>SaveChangesAsync</c> is never called on the query path.
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse>;
