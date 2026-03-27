using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollSettings;
using SplashSphere.Application.Features.Payroll.Queries.GetPayrollSettings;

namespace SplashSphere.API.Endpoints;

public static class PayrollSettingsEndpoints
{
    public static IEndpointRouteBuilder MapPayrollSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/settings")
            .RequireAuthorization()
            .WithTags("Settings");

        group.MapGet("/payroll-config", GetPayrollSettings);
        group.MapPut("/payroll-config", UpdatePayrollSettings);

        return app;
    }

    private static async Task<Ok<object>> GetPayrollSettings(
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetPayrollSettingsQuery(), ct);
        return TypedResults.Ok<object>(result);
    }

    private static async Task<Results<NoContent, BadRequest<ProblemDetails>>> UpdatePayrollSettings(
        [FromBody] UpdatePayrollSettingsRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdatePayrollSettingsCommand(
            body.CutOffStartDay, body.Frequency, body.PayReleaseDayOffset, body.AutoCalcGovernmentDeductions);

        var result = await sender.Send(command, ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

        return TypedResults.NoContent();
    }

    private sealed record UpdatePayrollSettingsRequest(
        int CutOffStartDay,
        int Frequency = 1,
        int PayReleaseDayOffset = 3,
        bool AutoCalcGovernmentDeductions = false);
}
