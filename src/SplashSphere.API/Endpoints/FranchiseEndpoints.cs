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
        group.MapGet("/settings",  GetSettings)    .WithName("GetFranchiseSettings").WithSummary("Get franchise network settings");
        group.MapPut("/settings",  UpdateSettings)  .WithName("UpdateFranchiseSettings").WithSummary("Update franchise network settings");

        // ── Franchisor: Franchisees ──────────────────────────────────────────────
        group.MapGet("/franchisees",             GetFranchisees)      .WithName("GetFranchisees").WithSummary("List all franchisees");
        group.MapGet("/franchisees/{id}",        GetFranchiseeDetail) .WithName("GetFranchiseeDetail").WithSummary("Get franchisee detail with agreement and royalties");
        group.MapPost("/franchisees/{id}/suspend",     SuspendFranchisee)   .WithName("SuspendFranchisee").WithSummary("Suspend a franchisee");
        group.MapPost("/franchisees/{id}/reactivate",  ReactivateFranchisee).WithName("ReactivateFranchisee").WithSummary("Reactivate a suspended franchisee");
        group.MapPost("/franchisees/{id}/push-templates", PushTemplates)     .WithName("PushServiceTemplates").WithSummary("Push service templates to a franchisee");

        // ── Franchisor: Agreements ───────────────────────────────────────────────
        group.MapPost("/agreements", CreateAgreement).WithName("CreateFranchiseAgreement").WithSummary("Create a franchise agreement");

        // ── Franchisor: Royalties ────────────────────────────────────────────────
        group.MapGet("/royalties",              GetRoyalties)      .WithName("GetRoyaltyPeriods").WithSummary("List royalty periods");
        group.MapPost("/royalties/calculate",   CalculateRoyalties).WithName("CalculateRoyalties").WithSummary("Calculate royalties for a period");
        group.MapPatch("/royalties/{id}/paid",  MarkPaid)          .WithName("MarkRoyaltyPaid").WithSummary("Mark a royalty period as paid");

        // ── Franchisor: Network ──────────────────────────────────────────────────
        group.MapGet("/network-summary", GetNetworkSummary).WithName("GetNetworkSummary").WithSummary("Get franchise network KPI summary");
        group.MapGet("/compliance",      GetCompliance)    .WithName("GetComplianceReport").WithSummary("Get compliance report per franchisee");

        // ── Franchisor: Service Templates ────────────────────────────────────────
        group.MapGet("/templates",       GetTemplates)     .WithName("GetServiceTemplates").WithSummary("List service templates");
        group.MapPost("/templates",      CreateTemplate)   .WithName("CreateServiceTemplate").WithSummary("Create a service template");
        group.MapPut("/templates/{id}",  UpdateTemplate)   .WithName("UpdateServiceTemplate").WithSummary("Update a service template");

        // ── Franchisor: Invitations ──────────────────────────────────────────────
        group.MapPost("/invite", InviteFranchisee).WithName("InviteFranchisee").WithSummary("Send a franchise invitation");

        // ── Franchisee: My data ──────────────────────────────────────────────────
        group.MapGet("/my-agreement", GetMyAgreement).WithName("GetMyAgreement").WithSummary("Get my franchise agreement");
        group.MapGet("/my-royalties", GetMyRoyalties).WithName("GetMyRoyalties").WithSummary("Get my royalty statements");
        group.MapGet("/benchmarks",   GetBenchmarks) .WithName("GetBenchmarks").WithSummary("Get performance benchmarks vs network");

        // ── Invitations (mixed auth) ─────────────────────────────────────────────
        // Validate is public (no auth), Accept requires auth.
        app.MapGet("/api/v1/franchise/invitations/{token}/validate", ValidateInvitation)
            .WithTags("Franchise")
            .WithName("ValidateInvitation")
            .WithSummary("Validate a franchise invitation token");

        app.MapPost("/api/v1/franchise/invitations/{token}/accept", AcceptInvitation)
            .WithTags("Franchise")
            .RequireAuthorization()
            .WithName("AcceptInvitation")
            .WithSummary("Accept a franchise invitation and create franchisee");

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
