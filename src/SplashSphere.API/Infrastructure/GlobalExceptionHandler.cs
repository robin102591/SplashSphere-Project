using Microsoft.AspNetCore.Diagnostics;
using FluentValidationException = FluentValidation.ValidationException;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.API.Infrastructure;

/// <summary>
/// Maps domain exceptions to RFC 9457 ProblemDetails HTTP responses.
/// Registered via <c>builder.Services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;()</c>
/// and activated by <c>app.UseExceptionHandler()</c>. Status-code mapping is
/// centralized in <see cref="ProblemDetailsMapper"/> so this handler and the
/// <c>Result.ToProblem()</c> extension can never drift apart.
/// </summary>
internal sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (errorCode, detail) = exception switch
        {
            FluentValidationException ve =>
                ("VALIDATION", string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),

            SplashSphereException se => (se.ErrorCode, se.Message),

            _ => ("INTERNAL_ERROR", "An unexpected error occurred."),
        };

        var statusCode = exception is SplashSphereException or FluentValidationException
            ? ProblemDetailsMapper.StatusFor(errorCode)
            : StatusCodes.Status500InternalServerError;

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext    = httpContext,
            Exception      = exception,
            ProblemDetails =
            {
                Title  = errorCode,
                Detail = detail,
                Status = statusCode,
            },
        });
    }
}
