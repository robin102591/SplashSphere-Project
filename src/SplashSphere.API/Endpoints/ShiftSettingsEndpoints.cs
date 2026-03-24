using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.Shifts.Commands.UpdateShiftSettings;
using SplashSphere.Application.Features.Shifts.Queries.GetShiftSettings;

namespace SplashSphere.API.Endpoints;

public static class ShiftSettingsEndpoints
{
    public static IEndpointRouteBuilder MapShiftSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/settings")
            .RequireAuthorization()
            .WithTags("Settings");

        group.MapGet("/shift-config", GetShiftSettings);
        group.MapPut("/shift-config", UpdateShiftSettings);

        return app;
    }

    private static async Task<Ok<object>> GetShiftSettings(
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetShiftSettingsQuery(), ct);
        return TypedResults.Ok<object>(result);
    }

    private static async Task<Results<NoContent, BadRequest<ProblemDetails>>> UpdateShiftSettings(
        [FromBody] UpdateShiftSettingsRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateShiftSettingsCommand(
            body.DefaultOpeningFund,
            body.AutoApproveThreshold,
            body.FlagThreshold,
            body.RequireShiftForTransactions,
            body.EndOfDayReminderTime,
            body.LockTimeoutMinutes,
            body.MaxPinAttempts);

        var result = await sender.Send(command, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

        return TypedResults.NoContent();
    }

    private sealed record UpdateShiftSettingsRequest(
        decimal DefaultOpeningFund,
        decimal AutoApproveThreshold,
        decimal FlagThreshold,
        bool RequireShiftForTransactions,
        TimeOnly EndOfDayReminderTime,
        int LockTimeoutMinutes,
        int MaxPinAttempts);
}
