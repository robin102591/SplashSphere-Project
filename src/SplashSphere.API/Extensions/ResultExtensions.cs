using Microsoft.AspNetCore.Mvc;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.API.Extensions;

/// <summary>
/// Maps a failed <see cref="Result"/> to an RFC 9457 ProblemDetails HTTP response.
/// Only call when <see cref="Result.IsFailure"/> is true.
/// </summary>
internal static class ResultExtensions
{
    internal static IResult ToProblem(this Result result)
    {
        var statusCode = result.Error.Code switch
        {
            "NOT_FOUND"    => StatusCodes.Status404NotFound,
            "CONFLICT"     => StatusCodes.Status409Conflict,
            "VALIDATION"   => StatusCodes.Status422UnprocessableEntity,
            "UNAUTHORIZED" => StatusCodes.Status401Unauthorized,
            "FORBIDDEN"    => StatusCodes.Status403Forbidden,
            _              => StatusCodes.Status400BadRequest,
        };

        return TypedResults.Problem(new ProblemDetails
        {
            Title  = result.Error.Code,
            Detail = result.Error.Message,
            Status = statusCode,
        });
    }
}
