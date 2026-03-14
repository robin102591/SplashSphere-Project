using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Common.Behaviors;

/// <summary>
/// Outermost pipeline behavior — wraps every request with structured timing logs.
/// Logs at <c>Warning</c> when the handler returns a <see cref="Result"/> failure,
/// and at <c>Error</c> when an unhandled exception escapes.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next(cancellationToken);
            sw.Stop();

            if (response is Result { IsFailure: true } failure)
            {
                logger.LogWarning(
                    "Handled {RequestName} with failure [{ErrorCode}: {ErrorMessage}] in {ElapsedMs}ms",
                    requestName, failure.Error.Code, failure.Error.Message, sw.ElapsedMilliseconds);
            }
            else
            {
                logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMs}ms",
                    requestName, sw.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex,
                "Request {RequestName} threw an unhandled exception after {ElapsedMs}ms",
                requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
