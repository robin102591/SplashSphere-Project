using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.CashAdvances;
using SplashSphere.Application.Features.CashAdvances.Commands.ApproveCashAdvance;
using SplashSphere.Application.Features.CashAdvances.Commands.CancelCashAdvance;
using SplashSphere.Application.Features.CashAdvances.Commands.CreateCashAdvance;
using SplashSphere.Application.Features.CashAdvances.Commands.DisburseCashAdvance;
using SplashSphere.Application.Features.CashAdvances.Queries.GetCashAdvanceById;
using SplashSphere.Application.Features.CashAdvances.Queries.GetCashAdvances;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class CashAdvanceEndpoints
{
    public static IEndpointRouteBuilder MapCashAdvanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/cash-advances")
            .RequireAuthorization()
            .WithTags("CashAdvances")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.CashAdvanceTracking));

        group.MapGet("/",             GetCashAdvances).WithSummary("List cash advances");
        group.MapGet("/{id}",         GetCashAdvanceById).WithSummary("Get cash advance by ID");
        group.MapPost("/",            CreateCashAdvance).WithSummary("Create cash advance");
        group.MapPatch("/{id}/approve",  ApproveCashAdvance).WithSummary("Approve cash advance");
        group.MapPatch("/{id}/disburse", DisburseCashAdvance).WithSummary("Disburse cash advance");
        group.MapPatch("/{id}/cancel",   CancelCashAdvance).WithSummary("Cancel cash advance");

        return app;
    }

    // ── GET / ───────────────────────────────────────────────────────────────

    private static async Task<Ok<object>> GetCashAdvances(
        [AsParameters] GetAdvancesParams p,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetCashAdvancesQuery(p.Page, p.PageSize, p.EmployeeId, p.Status), ct);
        return TypedResults.Ok<object>(result);
    }

    // ── GET /{id} ───────────────────────────────────────────────────────────

    private static async Task<Results<Ok<object>, NotFound>> GetCashAdvanceById(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetCashAdvanceByIdQuery(id), ct);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok<object>(result);
    }

    // ── POST / ──────────────────────────────────────────────────────────────

    private static async Task<Results<Created<object>, BadRequest<ProblemDetails>>> CreateCashAdvance(
        [FromBody] CreateAdvanceRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateCashAdvanceCommand(body.EmployeeId, body.Amount, body.DeductionPerPeriod, body.Reason), ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

        return TypedResults.Created($"/api/v1/cash-advances/{result.Value}", (object)new { id = result.Value });
    }

    // ── PATCH /{id}/approve ─────────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> ApproveCashAdvance(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ApproveCashAdvanceCommand(id), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── PATCH /{id}/disburse ────────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> DisburseCashAdvance(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DisburseCashAdvanceCommand(id), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── PATCH /{id}/cancel ──────────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> CancelCashAdvance(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CancelCashAdvanceCommand(id), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── Request / query-param records ───────────────────────────────────────

    private sealed record GetAdvancesParams(
        int Page = 1,
        int PageSize = 20,
        string? EmployeeId = null,
        CashAdvanceStatus? Status = null);

    private sealed record CreateAdvanceRequest(
        string EmployeeId,
        decimal Amount,
        decimal DeductionPerPeriod,
        string? Reason);
}
