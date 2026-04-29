using MediatR;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Display.Queries.GetCurrentDisplayTransaction;
using SplashSphere.Application.Features.Display.Queries.GetDisplayConfig;

namespace SplashSphere.API.Endpoints;

/// <summary>
/// Endpoints consumed by the customer-facing display device and by the POS
/// app to control what the paired display is showing. Read-only data lives at
/// <c>/config</c> and <c>/current</c>; the <c>/show</c> and <c>/clear</c>
/// actions exist because the cashier moves between transactions in ways that
/// don't necessarily fire a transaction-lifecycle event (e.g. opening an
/// existing Pay Later transaction, parking a transaction and walking away).
/// </summary>
public static class DisplayEndpoints
{
    public static IEndpointRouteBuilder MapDisplayEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/display")
            .RequireAuthorization()
            .WithTags("Customer Display");

        group.MapGet("/config", GetDisplayConfig)
            .WithName("GetDisplayConfig")
            .WithSummary("Combined render config for the display device: settings (with branch fallback) + customer-safe tenant branding.");

        group.MapGet("/current", GetCurrentTransaction)
            .WithName("GetCurrentDisplayTransaction")
            .WithSummary("Returns the in-progress transaction for a station (or null) so the display can rebuild after a SignalR reconnect.");

        group.MapPost("/show/{transactionId}", ShowTransaction)
            .WithName("ShowDisplayTransaction")
            .WithSummary("Pushes a transaction to its station's customer display, regardless of lifecycle event timing. Used when the cashier opens an existing transaction page.");

        group.MapPost("/clear", ClearDisplay)
            .WithName("ClearDisplay")
            .WithSummary("Reverts a station's customer display to Idle without touching any transaction. Used when the cashier parks a transaction (Pay Later) and walks away.");

        return app;
    }

    private static async Task<IResult> GetDisplayConfig(
        [FromQuery] string? branchId,
        ISender sender,
        CancellationToken ct)
    {
        var config = await sender.Send(new GetDisplayConfigQuery(branchId), ct);
        return TypedResults.Ok(config);
    }

    private static async Task<IResult> GetCurrentTransaction(
        [FromQuery] string branchId,
        [FromQuery] string stationId,
        ISender sender,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(branchId) || string.IsNullOrWhiteSpace(stationId))
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title  = "VALIDATION",
                Detail = "branchId and stationId are required.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        var result = await sender.Send(
            new GetCurrentDisplayTransactionQuery(branchId, stationId), ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> ShowTransaction(
        string transactionId,
        IDisplayBroadcaster broadcaster,
        CancellationToken ct)
    {
        // The broadcaster reads the transaction's PosStationId to determine
        // which group to dispatch to, so this works for any transaction the
        // current tenant owns (the global query filter on Transactions
        // enforces tenant isolation — a foreign tenant's transactionId returns
        // null and the broadcast becomes a no-op).
        await broadcaster.BroadcastUpdatedAsync(transactionId, ct);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> ClearDisplay(
        [FromBody] ClearDisplayRequest body,
        IDisplayBroadcaster broadcaster,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.BranchId) || string.IsNullOrWhiteSpace(body.StationId))
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Title  = "VALIDATION",
                Detail = "branchId and stationId are required.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        await broadcaster.ClearStationAsync(body.BranchId, body.StationId, ct);
        return TypedResults.NoContent();
    }

    private sealed record ClearDisplayRequest(string BranchId, string StationId);
}
