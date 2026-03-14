using MediatR;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Internal marker so <c>UnitOfWorkBehavior</c> can detect commands at runtime
/// via a single <c>is IBaseCommand</c> check, without requiring a CLR generic
/// constraint that would break open-generic DI registration.
/// </summary>
public interface IBaseCommand;

/// <summary>Command that returns no domain value — only success or failure.</summary>
public interface ICommand : IRequest<Result>, IBaseCommand;

/// <summary>Command that returns a domain value on success.</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;
