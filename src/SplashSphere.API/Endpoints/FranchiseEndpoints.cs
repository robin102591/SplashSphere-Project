using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Franchise;
using SplashSphere.Application.Features.Franchise.Commands.AcceptInvitation;
using SplashSphere.Application.Features.Franchise.Commands.CalculateRoyalties;
using SplashSphere.Application.Features.Franchise.Commands.CreateFranchiseAgreement;
using SplashSphere.Application.Features.Franchise.Commands.InviteFranchisee;
using SplashSphere.Application.Features.Franchise.Commands.MarkRoyaltyPaid;
using SplashSphere.Application.Features.Franchise.Commands.PushServiceTemplates;
using SplashSphere.Application.Features.Franchise.Commands.ReactivateFranchisee;
using SplashSphere.Application.Features.Franchise.Commands.SuspendFranchisee;
using SplashSphere.Application.Features.Franchise.Commands.UpdateFranchiseSettings;
using SplashSphere.Application.Features.Franchise.Commands.UpsertServiceTemplate;
using SplashSphere.Application.Features.Franchise.Commands.ValidateInvitation;
using SplashSphere.Application.Features.Franchise.Queries.GetBenchmarks;
using SplashSphere.Application.Features.Franchise.Queries.GetComplianceReport;
using SplashSphere.Application.Features.Franchise.Queries.GetFranchiseeDetail;
using SplashSphere.Application.Features.Franchise.Queries.GetFranchisees;
using SplashSphere.Application.Features.Franchise.Queries.GetFranchiseSettings;
using SplashSphere.Application.Features.Franchise.Queries.GetMyAgreement;
using SplashSphere.Application.Features.Franchise.Queries.GetMyRoyalties;
using SplashSphere.Application.Features.Franchise.Queries.GetNetworkSummary;
using SplashSphere.Application.Features.Franchise.Queries.GetRoyaltyPeriods;
using SplashSphere.Application.Features.Franchise.Queries.GetServiceTemplates;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class FranchiseEndpoints
{
    public static IEndpointRouteBuilder MapFranchiseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/franchise")
            .WithTags("Franchise")
            .RequireAuthorization()
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.FranchiseManagement));

        // ── Franchisor: Settings ─────────────────────────────────────────────────
        group.MapGet("/settings",  GetSettings)    .WithName("GetFranchiseSettings");
        group.MapPut("/settings",  UpdateSettings)  .WithName("UpdateFranchiseSettings");

        // ── Franchisor: Franchisees ──────────────────────────────────────────────
        group.MapGet("/franchisees",             GetFranchisees)      .WithName("GetFranchisees");
        group.MapGet("/franchisees/{id}",        GetFranchiseeDetail) .WithName("GetFranchiseeDetail");
        group.MapPost("/franchisees/{id}/suspend",     SuspendFranchisee)   .WithName("SuspendFranchisee");
        group.MapPost("/franchisees/{id}/reactivate",  ReactivateFranchisee).WithName("ReactivateFranchisee");
        group.MapPost("/franchisees/{id}/push-templates", PushTemplates)     .WithName("PushServiceTemplates");

        // ── Franchisor: Agreements ───────────────────────────────────────────────
        group.MapPost("/agreements", CreateAgreement).WithName("CreateFranchiseAgreement");

        // ── Franchisor: Royalties ────────────────────────────────────────────────
        group.MapGet("/royalties",              GetRoyalties)      .WithName("GetRoyaltyPeriods");
        group.MapPost("/royalties/calculate",   CalculateRoyalties).WithName("CalculateRoyalties");
        group.MapPatch("/royalties/{id}/paid",  MarkPaid)          .WithName("MarkRoyaltyPaid");

        // ── Franchisor: Network ──────────────────────────────────────────────────
        group.MapGet("/network-summary", GetNetworkSummary).WithName("GetNetworkSummary");
        group.MapGet("/compliance",      GetCompliance)    .WithName("GetComplianceReport");

        // ── Franchisor: Service Templates ────────────────────────────────────────
        group.MapGet("/templates",       GetTemplates)     .WithName("GetServiceTemplates");
        group.MapPost("/templates",      CreateTemplate)   .WithName("CreateServiceTemplate");
        group.MapPut("/templates/{id}",  UpdateTemplate)   .WithName("UpdateServiceTemplate");

        // ── Franchisor: Invitations ──────────────────────────────────────────────
        group.MapPost("/invite", InviteFranchisee).WithName("InviteFranchisee");

        // ── Franchisee: My data ──────────────────────────────────────────────────
        group.MapGet("/my-agreement", GetMyAgreement).WithName("GetMyAgreement");
        group.MapGet("/my-royalties", GetMyRoyalties).WithName("GetMyRoyalties");
        group.MapGet("/benchmarks",   GetBenchmarks) .WithName("GetBenchmarks");

        // ── Invitations (mixed auth) ─────────────────────────────────────────────
        // Validate is public (no auth), Accept requires auth.
        app.MapGet("/api/v1/franchise/invitations/{token}/validate", ValidateInvitation)
            .WithTags("Franchise")
            .WithName("ValidateInvitation");

        app.MapPost("/api/v1/franchise/invitations/{token}/accept", AcceptInvitation)
            .WithTags("Franchise")
            .RequireAuthorization()
            .WithName("AcceptInvitation");

        return app;
    }

    // ── Franchisor: Settings ─────────────────────────────────────────────────────

    private static async Task<IResult> GetSettings(
        ISender sender,
        CancellationToken ct)
    {
        var dto = await sender.Send(new GetFranchiseSettingsQuery(), ct);
        return TypedResults.Ok(dto);
    }

    private static async Task<IResult> UpdateSettings(
        UpdateFranchiseSettingsCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── Franchisor: Franchisees ──────────────────────────────────────────────────

    private static async Task<IResult> GetFranchisees(
        [AsParameters] GetFranchiseesQuery query,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetFranchiseeDetail(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var dto = await sender.Send(new GetFranchiseeDetailQuery(id), ct);
        return TypedResults.Ok(dto);
    }

    private static async Task<IResult> SuspendFranchisee(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new SuspendFranchiseeCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> ReactivateFranchisee(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ReactivateFranchiseeCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> PushTemplates(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new PushServiceTemplatesCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── Franchisor: Agreements ───────────────────────────────────────────────────

    private static async Task<IResult> CreateAgreement(
        CreateFranchiseAgreementCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/franchise/agreements/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    // ── Franchisor: Royalties ────────────────────────────────────────────────────

    private static async Task<IResult> GetRoyalties(
        [AsParameters] GetRoyaltyPeriodsQuery query,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> CalculateRoyalties(
        CalculateRoyaltiesCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/franchise/royalties/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> MarkPaid(
        string id,
        MarkRoyaltyPaidRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new MarkRoyaltyPaidCommand(id, body.PaymentReference), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── Franchisor: Network ──────────────────────────────────────────────────────

    private static async Task<IResult> GetNetworkSummary(
        ISender sender,
        CancellationToken ct)
    {
        var dto = await sender.Send(new GetNetworkSummaryQuery(), ct);
        return TypedResults.Ok(dto);
    }

    private static async Task<IResult> GetCompliance(
        ISender sender,
        CancellationToken ct)
    {
        var items = await sender.Send(new GetComplianceReportQuery(), ct);
        return TypedResults.Ok(items);
    }

    // ── Franchisor: Service Templates ────────────────────────────────────────────

    private static async Task<IResult> GetTemplates(
        ISender sender,
        CancellationToken ct)
    {
        var items = await sender.Send(new GetServiceTemplatesQuery(), ct);
        return TypedResults.Ok(items);
    }

    private static async Task<IResult> CreateTemplate(
        UpsertServiceTemplateCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command with { Id = null }, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/franchise/templates/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> UpdateTemplate(
        string id,
        UpdateTemplateRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpsertServiceTemplateCommand(
            id,
            body.ServiceName,
            body.Description,
            body.CategoryName,
            body.BasePrice,
            body.DurationMinutes,
            body.IsRequired,
            body.PricingMatrixJson,
            body.CommissionMatrixJson);

        var result = await sender.Send(command, ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── Franchisor: Invitations ──────────────────────────────────────────────────

    private static async Task<IResult> InviteFranchisee(
        InviteFranchiseeCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/franchise/invitations/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    // ── Franchisee: My data ──────────────────────────────────────────────────────

    private static async Task<IResult> GetMyAgreement(
        ISender sender,
        CancellationToken ct)
    {
        var dto = await sender.Send(new GetMyAgreementQuery(), ct);
        return TypedResults.Ok(dto);
    }

    private static async Task<IResult> GetMyRoyalties(
        [AsParameters] GetMyRoyaltiesQuery query,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetBenchmarks(
        ISender sender,
        CancellationToken ct)
    {
        var items = await sender.Send(new GetBenchmarksQuery(), ct);
        return TypedResults.Ok(items);
    }

    // ── Invitation validation & acceptance ───────────────────────────────────────

    private static async Task<IResult> ValidateInvitation(
        string token,
        ISender sender,
        CancellationToken ct)
    {
        var dto = await sender.Send(new ValidateInvitationQuery(token), ct);
        return TypedResults.Ok(dto);
    }

    private static async Task<IResult> AcceptInvitation(
        string token,
        AcceptInvitationRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var command = new AcceptInvitationCommand(
            token,
            body.BusinessName,
            body.Email,
            body.ContactNumber,
            body.Address,
            body.BranchName,
            body.BranchCode,
            body.BranchAddress,
            body.BranchContactNumber);

        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/franchise/my-agreement", new { id = result.Value })
            : result.ToProblem();
    }

    // ── Request bodies (route id comes from path, not body) ─────────────────────

    private sealed record MarkRoyaltyPaidRequest(string? PaymentReference);

    private sealed record UpdateTemplateRequest(
        string ServiceName,
        string? Description,
        string? CategoryName,
        decimal BasePrice,
        int DurationMinutes,
        bool IsRequired,
        string? PricingMatrixJson,
        string? CommissionMatrixJson);

    private sealed record AcceptInvitationRequest(
        string BusinessName,
        string Email,
        string ContactNumber,
        string Address,
        string BranchName,
        string BranchCode,
        string BranchAddress,
        string BranchContactNumber);
}
