using MediatR;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Connect.Loyalty.Commands.RedeemReward;
using SplashSphere.Application.Features.Connect.Loyalty.Queries.GetMyLoyalty;
using SplashSphere.Application.Features.Connect.Loyalty.Queries.GetPointsHistory;
using SplashSphere.Application.Features.Connect.Loyalty.Queries.GetRewards;

namespace SplashSphere.API.Endpoints.Connect;

/// <summary>
/// Connect app loyalty endpoints under <c>/api/v1/connect/carwashes/{tenantId}</c>.
/// All require a valid Connect JWT. Tenant-plan feature gating is enforced inside
/// the handlers so callers see a degraded (not errored) response when the tenant
/// doesn't offer loyalty.
/// </summary>
public static class ConnectLoyaltyEndpoints
{
    public static IEndpointRouteBuilder MapConnectLoyaltyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapConnectGroup("/api/v1/connect/carwashes", "Connect.Loyalty");

        // GET /api/v1/connect/carwashes/{tenantId}/loyalty
        group.MapGet("/{tenantId}/loyalty", async (
            string tenantId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetMyLoyaltyQuery(tenantId), ct);
            return Results.Ok(result);
        })
        .WithName("Connect.GetMyLoyalty")
        .WithSummary("Get the caller's loyalty membership at a car wash.");

        // GET /api/v1/connect/carwashes/{tenantId}/rewards
        group.MapGet("/{tenantId}/rewards", async (
            string tenantId,
            ISender sender,
            CancellationToken ct) =>
        {
            var items = await sender.Send(new GetRewardsQuery(tenantId), ct);
            return Results.Ok(items);
        })
        .WithName("Connect.GetRewards")
        .WithSummary("List loyalty rewards offered by a car wash with affordability flags.");

        // POST /api/v1/connect/carwashes/{tenantId}/rewards/redeem
        group.MapPost("/{tenantId}/rewards/redeem", async (
            string tenantId,
            [FromBody] RedeemRewardRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RedeemRewardCommand(tenantId, body.RewardId), ct);
            return result.IsFailure ? result.ToProblem() : Results.Ok(result.Value);
        })
        .WithName("Connect.RedeemReward")
        .WithSummary("Redeem points for a reward — emits a redemption ledger entry.");

        // GET /api/v1/connect/carwashes/{tenantId}/points-history?take=50
        group.MapGet("/{tenantId}/points-history", async (
            string tenantId,
            int? take,
            ISender sender,
            CancellationToken ct) =>
        {
            var items = await sender.Send(new GetPointsHistoryQuery(tenantId, take), ct);
            return Results.Ok(items);
        })
        .WithName("Connect.GetPointsHistory")
        .WithSummary("List the caller's point movements at a car wash, newest first.");

        return app;
    }
}

// ── Request bodies ──────────────────────────────────────────────────────────

internal sealed record RedeemRewardRequest(string RewardId);
