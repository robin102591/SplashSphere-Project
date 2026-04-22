using MediatR;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Connect.Referral.Commands.ApplyReferral;
using SplashSphere.Application.Features.Connect.Referral.Queries.GetMyReferrals;
using SplashSphere.Application.Features.Connect.Referral.Queries.GetReferralCode;

namespace SplashSphere.API.Endpoints.Connect;

/// <summary>
/// Connect app referral endpoints — per-tenant referral codes and claims.
/// Requires a valid Connect JWT.
/// </summary>
public static class ConnectReferralEndpoints
{
    public static IEndpointRouteBuilder MapConnectReferralEndpoints(this IEndpointRouteBuilder app)
    {
        var carwashes = app.MapConnectGroup("/api/v1/connect/carwashes", "Connect.Referral");

        // GET /api/v1/connect/carwashes/{tenantId}/referral-code
        carwashes.MapGet("/{tenantId}/referral-code", async (
            string tenantId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetReferralCodeQuery(tenantId), ct);
            return result.IsFailure ? result.ToProblem() : Results.Ok(result.Value);
        })
        .WithName("Connect.GetReferralCode")
        .WithSummary("Get (and lazily issue) the caller's referral code at a car wash.");

        // GET /api/v1/connect/carwashes/{tenantId}/referrals
        carwashes.MapGet("/{tenantId}/referrals", async (
            string tenantId,
            ISender sender,
            CancellationToken ct) =>
        {
            var items = await sender.Send(new GetMyReferralsQuery(tenantId), ct);
            return Results.Ok(items);
        })
        .WithName("Connect.GetMyReferrals")
        .WithSummary("List referrals the caller has made at a car wash.");

        // POST /api/v1/connect/carwashes/{tenantId}/apply-referral
        carwashes.MapPost("/{tenantId}/apply-referral", async (
            string tenantId,
            [FromBody] ApplyReferralRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ApplyReferralCommand(tenantId, body.Code), ct);
            return result.IsFailure ? result.ToProblem() : Results.Ok(result.Value);
        })
        .WithName("Connect.ApplyReferral")
        .WithSummary("Apply a referral code at a car wash the caller has joined.");

        return app;
    }
}

// ── Request bodies ──────────────────────────────────────────────────────────

internal sealed record ApplyReferralRequest(string Code);
