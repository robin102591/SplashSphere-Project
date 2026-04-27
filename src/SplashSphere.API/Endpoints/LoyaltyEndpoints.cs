using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Loyalty;
using SplashSphere.Application.Features.Loyalty.Commands.AdjustPoints;
using SplashSphere.Application.Features.Loyalty.Commands.CreateLoyaltyReward;
using SplashSphere.Application.Features.Loyalty.Commands.EnrollCustomer;
using SplashSphere.Application.Features.Loyalty.Commands.RedeemPoints;
using SplashSphere.Application.Features.Loyalty.Commands.ToggleLoyaltyRewardStatus;
using SplashSphere.Application.Features.Loyalty.Commands.UpdateLoyaltyReward;
using SplashSphere.Application.Features.Loyalty.Commands.UpsertLoyaltySettings;
using SplashSphere.Application.Features.Loyalty.Commands.UpsertLoyaltyTiers;
using SplashSphere.Application.Features.Loyalty.Queries.GetCustomerLoyaltySummary;
using SplashSphere.Application.Features.Loyalty.Queries.GetLoyaltyDashboard;
using SplashSphere.Application.Features.Loyalty.Queries.GetLoyaltyRewards;
using SplashSphere.Application.Features.Loyalty.Queries.GetLoyaltySettings;
using SplashSphere.Application.Features.Loyalty.Queries.GetMembershipCard;
using SplashSphere.Application.Features.Loyalty.Queries.GetMembershipCardByNumber;
using SplashSphere.Application.Features.Loyalty.Queries.GetPointHistory;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class LoyaltyEndpoints
{
    public static IEndpointRouteBuilder MapLoyaltyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/loyalty")
            .RequireAuthorization()
            .WithTags("Loyalty")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.CustomerLoyalty));

        // ── Settings & config ───────────────────────────────────────────────
        group.MapGet("/settings", GetSettings).WithSummary("Get loyalty program settings and tier configurations");
        group.MapPut("/settings", UpsertSettings).WithSummary("Upsert loyalty program settings");
        group.MapPut("/tiers", UpsertTiers).WithSummary("Upsert loyalty tier configurations");

        // ── Rewards catalogue ───────────────────────────────────────────────
        group.MapGet("/rewards", GetRewards).WithSummary("List loyalty rewards");
        group.MapPost("/rewards", CreateReward).WithSummary("Create a loyalty reward");
        group.MapPut("/rewards/{id}", UpdateReward).WithSummary("Update a loyalty reward");
        group.MapPatch("/rewards/{id}/status", ToggleRewardStatus).WithSummary("Toggle reward active or inactive");

        // ── Dashboard ───────────────────────────────────────────────────────
        group.MapGet("/dashboard", GetDashboard).WithSummary("Get loyalty dashboard with members, points, and tier stats");

        // ── Members ─────────────────────────────────────────────────────────
        group.MapPost("/members", Enroll).WithSummary("Enroll a customer in the loyalty program");
        group.MapGet("/members/by-customer/{customerId}", GetByCustomer).WithSummary("Get membership card by customer ID");
        group.MapGet("/members/by-card/{cardNumber}", GetByCardNumber).WithSummary("Get membership card by card number");
        group.MapGet("/members/{membershipCardId}/points", GetPointHistory).WithSummary("Get point history for a membership card");
        group.MapPost("/members/{membershipCardId}/redeem", Redeem).WithSummary("Redeem points for a reward");
        group.MapPost("/members/{membershipCardId}/adjust", Adjust).WithSummary("Manually adjust points on a membership card");
        group.MapGet("/members/by-customer/{customerId}/summary", GetSummary).WithSummary("Get lightweight loyalty summary for POS");

        return app;
    }

    // ── Settings ────────────────────────────────────────────────────────────

    private static async Task<IResult> GetSettings(ISender sender, CancellationToken ct)
        => TypedResults.Ok(await sender.Send(new GetLoyaltySettingsQuery(), ct));

    private static async Task<IResult> UpsertSettings(
        [FromBody] UpsertSettingsRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpsertLoyaltySettingsCommand(
            body.PointsPerCurrencyUnit, body.CurrencyUnitAmount,
            body.IsActive, body.PointsExpirationMonths, body.AutoEnroll), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> UpsertTiers(
        [FromBody] UpsertTiersRequest body, ISender sender, CancellationToken ct)
    {
        var tiers = body.Tiers.Select(t => new TierInput(t.Tier, t.Name, t.MinimumLifetimePoints, t.PointsMultiplier)).ToList();
        var result = await sender.Send(new UpsertLoyaltyTiersCommand(tiers), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── Rewards ─────────────────────────────────────────────────────────────

    private static async Task<IResult> GetRewards(
        [AsParameters] RewardListParams p, ISender sender, CancellationToken ct)
        => TypedResults.Ok(await sender.Send(
            new GetLoyaltyRewardsQuery(p.ActiveOnly, p.Page, p.PageSize), ct));

    private static async Task<IResult> CreateReward(
        [FromBody] CreateRewardRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreateLoyaltyRewardCommand(
            body.Name, body.Description, body.RewardType, body.PointsCost,
            body.ServiceId, body.PackageId, body.DiscountAmount, body.DiscountPercent), ct);

        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/loyalty/rewards/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> UpdateReward(
        string id, [FromBody] UpdateRewardRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateLoyaltyRewardCommand(
            id, body.Name, body.Description, body.RewardType, body.PointsCost,
            body.ServiceId, body.PackageId, body.DiscountAmount, body.DiscountPercent), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<Results<NoContent, NotFound>> ToggleRewardStatus(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ToggleLoyaltyRewardStatusCommand(id), ct);
        return result.IsFailure ? TypedResults.NotFound() : TypedResults.NoContent();
    }

    // ── Dashboard ───────────────────────────────────────────────────────────

    private static async Task<IResult> GetDashboard(
        DateTime from, DateTime to, ISender sender, CancellationToken ct)
        => TypedResults.Ok(await sender.Send(new GetLoyaltyDashboardQuery(from, to), ct));

    // ── Members ─────────────────────────────────────────────────────────────

    private static async Task<IResult> Enroll(
        [FromBody] EnrollRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new EnrollCustomerCommand(body.CustomerId), ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/loyalty/members/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> GetByCustomer(
        string customerId, ISender sender, CancellationToken ct)
        => TypedResults.Ok(await sender.Send(new GetMembershipCardQuery(customerId), ct));

    private static async Task<IResult> GetByCardNumber(
        string cardNumber, ISender sender, CancellationToken ct)
        => TypedResults.Ok(await sender.Send(new GetMembershipCardByNumberQuery(cardNumber), ct));

    private static async Task<IResult> GetPointHistory(
        string membershipCardId, int page, int pageSize, ISender sender, CancellationToken ct)
        => TypedResults.Ok(await sender.Send(
            new GetPointHistoryQuery(membershipCardId, page, pageSize), ct));

    private static async Task<IResult> Redeem(
        string membershipCardId, [FromBody] RedeemRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RedeemPointsCommand(
            membershipCardId, body.RewardId, body.TransactionId), ct);

        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> Adjust(
        string membershipCardId, [FromBody] AdjustRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new AdjustPointsCommand(membershipCardId, body.Points, body.Reason), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> GetSummary(
        string customerId, ISender sender, CancellationToken ct)
        => TypedResults.Ok(await sender.Send(new GetCustomerLoyaltySummaryQuery(customerId), ct));

    // ── Request records ─────────────────────────────────────────────────────

    private sealed record UpsertSettingsRequest(
        decimal PointsPerCurrencyUnit, decimal CurrencyUnitAmount,
        bool IsActive, int? PointsExpirationMonths, bool AutoEnroll);

    private sealed record UpsertTiersRequest(IReadOnlyList<TierInputRequest> Tiers);

    private sealed record TierInputRequest(
        Domain.Enums.LoyaltyTier Tier, string Name,
        int MinimumLifetimePoints, decimal PointsMultiplier);

    private sealed record RewardListParams(bool? ActiveOnly = null, int Page = 1, int PageSize = 50);

    private sealed record CreateRewardRequest(
        string Name, string? Description, Domain.Enums.RewardType RewardType,
        int PointsCost, string? ServiceId = null, string? PackageId = null,
        decimal? DiscountAmount = null, decimal? DiscountPercent = null);

    private sealed record UpdateRewardRequest(
        string Name, string? Description, Domain.Enums.RewardType RewardType,
        int PointsCost, string? ServiceId = null, string? PackageId = null,
        decimal? DiscountAmount = null, decimal? DiscountPercent = null);

    private sealed record EnrollRequest(string CustomerId);
    private sealed record RedeemRequest(string RewardId, string? TransactionId = null);
    private sealed record AdjustRequest(int Points, string Reason);
}
