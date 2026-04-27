using Microsoft.AspNetCore.Mvc;

namespace SplashSphere.API.Infrastructure;

/// <summary>
/// Single source of truth for translating an application error code into the
/// matching HTTP status. Used by both <see cref="GlobalExceptionHandler"/>
/// (for thrown <c>SplashSphereException</c>s) and the <c>Result.ToProblem()</c>
/// extension (for handler-returned <c>Result</c> failures), so the two paths
/// can never drift out of sync.
/// </summary>
internal static class ProblemDetailsMapper
{
    /// <summary>
    /// Returns the HTTP status code for the given error code. Unknown codes
    /// fall through to <see cref="StatusCodes.Status400BadRequest"/>.
    /// </summary>
    public static int StatusFor(string errorCode) => errorCode switch
    {
        "NOT_FOUND"    => StatusCodes.Status404NotFound,
        "CONFLICT"     => StatusCodes.Status409Conflict,
        "VALIDATION"   => StatusCodes.Status422UnprocessableEntity,
        "UNAUTHORIZED" => StatusCodes.Status401Unauthorized,
        "FORBIDDEN"    => StatusCodes.Status403Forbidden,
        _              => StatusCodes.Status400BadRequest,
    };

    /// <summary>
    /// Builds a ProblemDetails-shaped <see cref="IResult"/> for an error code
    /// + message pair. The endpoint extension delegates here.
    /// </summary>
    public static IResult Problem(string errorCode, string message) =>
        TypedResults.Problem(new ProblemDetails
        {
            Title  = errorCode,
            Detail = message,
            Status = StatusFor(errorCode),
        });
}
