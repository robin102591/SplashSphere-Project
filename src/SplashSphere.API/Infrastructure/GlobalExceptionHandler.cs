using Microsoft.AspNetCore.Diagnostics;
using FluentValidationException = FluentValidation.ValidationException;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.API.Infrastructure;

/// <summary>
/// Maps domain exceptions to RFC 9457 ProblemDetails HTTP responses.
/// Registered via <c>builder.Services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;()</c>
/// and activated by <c>app.UseExceptionHandler()</c>.
/// </summary>
internal sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        (int statusCode, string title, string? detail) = exception switch
        {
            FluentValidationException ve => (
                StatusCodes.Status422UnprocessableEntity,
                "VALIDATION",
                string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),

            NotFoundException e    => (StatusCodes.Status404NotFound,    e.ErrorCode, e.Message),
            ConflictException e    => (StatusCodes.Status409Conflict,    e.ErrorCode, e.Message),
            ForbiddenException e   => (StatusCodes.Status403Forbidden,   e.ErrorCode, e.Message),
            UnauthorizedException e => (StatusCodes.Status401Unauthorized, e.ErrorCode, e.Message),
            DomainException e      => (StatusCodes.Status400BadRequest,  e.ErrorCode, e.Message),

            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR",
                  "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext    = httpContext,
            Exception      = exception,
            ProblemDetails =
            {
                Title  = title,
                Detail = detail,
                Status = statusCode,
            },
        });
    }
}
