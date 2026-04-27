using SplashSphere.API.Infrastructure;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.API.Extensions;

/// <summary>
/// Maps a failed <see cref="Result"/> to an RFC 9457 ProblemDetails HTTP response.
/// Only call when <see cref="Result.IsFailure"/> is true. Status-code mapping
/// is centralized in <see cref="ProblemDetailsMapper"/>.
/// </summary>
internal static class ResultExtensions
{
    internal static IResult ToProblem(this Result result) =>
        ProblemDetailsMapper.Problem(result.Error.Code, result.Error.Message);
}
