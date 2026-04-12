using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.Shifts.Commands.CloseShift;
using SplashSphere.Application.Features.Shifts.Commands.OpenShift;
using SplashSphere.Application.Features.Shifts.Commands.RecordCashMovement;
using SplashSphere.Application.Features.Shifts.Commands.ReopenShift;
using SplashSphere.Application.Features.Shifts.Commands.ReviewShift;
using SplashSphere.Application.Features.Shifts.Commands.VoidShift;
using SplashSphere.Application.Features.Shifts.Queries.GetCurrentShift;
using SplashSphere.Application.Features.Shifts.Queries.GetShiftById;
using SplashSphere.Application.Features.Shifts.Queries.GetShiftReport;
using SplashSphere.Application.Features.Shifts.Queries.GetShifts;
using SplashSphere.Application.Features.Shifts.Queries.GetShiftVarianceReport;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class ShiftEndpoints
{
    public static IEndpointRouteBuilder MapShiftEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/shifts")
            .RequireAuthorization()
            .WithTags("Shifts")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.ShiftManagement));

        // Commands
        group.MapPost("/open",               OpenShift).WithSummary("Open cashier shift");
        group.MapPost("/{id}/cash-movement", RecordCashMovement).WithSummary("Record cash movement");
        group.MapPost("/{id}/close",         CloseShift).WithSummary("Close shift with denomination count");
        group.MapPatch("/{id}/review",       ReviewShift).WithSummary("Review shift");
        group.MapPatch("/{id}/reopen",       ReopenShift).WithSummary("Reopen closed shift");
        group.MapPatch("/{id}/void",         VoidShift).WithSummary("Void empty shift");

        // Queries
        group.MapGet("/current",             GetCurrentShift).WithSummary("Get current open shift");
        group.MapGet("/",                    GetShifts).WithSummary("List shifts");
        group.MapGet("/{id}",                GetShiftById).WithSummary("Get shift detail");
        group.MapGet("/{id}/report",         GetShiftReport).WithSummary("Get end-of-day report");
        group.MapGet("/variance-report",     GetVarianceReport).WithSummary("Get variance report");

        return app;
    }

    // ── POST /open ────────────────────────────────────────────────────────────

    private static async Task<Results<Created<OpenShiftResponse>, BadRequest<ProblemDetails>>> OpenShift(
        [FromBody] OpenShiftRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new OpenShiftCommand(body.BranchId, body.OpeningCashFund), ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

        return TypedResults.Created($"/api/v1/shifts/{result.Value}", new OpenShiftResponse(result.Value!));
    }

    // ── POST /{id}/cash-movement ──────────────────────────────────────────────

    private static async Task<Results<Created<RecordCashMovementResponse>, NotFound, BadRequest<ProblemDetails>>> RecordCashMovement(
        string id,
        [FromBody] RecordCashMovementRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new RecordCashMovementCommand(id, body.Type, body.Amount, body.Reason, body.Reference), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NOT_FOUND")
                return TypedResults.NotFound();

            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.Created(
            $"/api/v1/shifts/{id}/cash-movements/{result.Value}",
            new RecordCashMovementResponse(result.Value!));
    }

    // ── POST /{id}/close ──────────────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> CloseShift(
        string id,
        [FromBody] CloseShiftRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var denominations = body.Denominations
            .Select(d => new DenominationEntry(d.DenominationValue, d.Count))
            .ToList();

        var result = await sender.Send(new CloseShiftCommand(id, denominations), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NOT_FOUND")
                return TypedResults.NotFound();

            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── PATCH /{id}/review ────────────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> ReviewShift(
        string id,
        [FromBody] ReviewShiftRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ReviewShiftCommand(id, body.NewReviewStatus, body.Notes), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NOT_FOUND")
                return TypedResults.NotFound();

            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── PATCH /{id}/reopen ────────────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> ReopenShift(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ReopenShiftCommand(id), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NOT_FOUND")
                return TypedResults.NotFound();

            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── PATCH /{id}/void ──────────────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> VoidShift(
        string id,
        [FromBody] VoidShiftRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new VoidShiftCommand(id, body.Reason), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NOT_FOUND")
                return TypedResults.NotFound();

            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── GET /current ──────────────────────────────────────────────────────────

    private static async Task<Results<Ok<object>, NotFound>> GetCurrentShift(
        [AsParameters] GetCurrentShiftParams p,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetCurrentShiftQuery(p.BranchId ?? string.Empty), ct);

        return result is null
            ? TypedResults.NotFound()
            : TypedResults.Ok<object>(result);
    }

    // ── GET / ─────────────────────────────────────────────────────────────────

    private static async Task<Ok<object>> GetShifts(
        [AsParameters] GetShiftsParams p,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetShiftsQuery(
            p.BranchId,
            p.CashierId,
            p.DateFrom,
            p.DateTo,
            p.Status,
            p.ReviewStatus,
            p.Page,
            p.PageSize);

        var result = await sender.Send(query, ct);
        return TypedResults.Ok<object>(result);
    }

    // ── GET /{id} ─────────────────────────────────────────────────────────────

    private static async Task<Results<Ok<object>, NotFound>> GetShiftById(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetShiftByIdQuery(id), ct);

        return result is null
            ? TypedResults.NotFound()
            : TypedResults.Ok<object>(result);
    }

    // ── GET /{id}/report ──────────────────────────────────────────────────────

    private static async Task<Results<Ok<object>, NotFound>> GetShiftReport(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetShiftReportQuery(id), ct);

        return result is null
            ? TypedResults.NotFound()
            : TypedResults.Ok<object>(result);
    }

    // ── GET /variance-report ──────────────────────────────────────────────────

    private static async Task<Ok<object>> GetVarianceReport(
        [AsParameters] GetVarianceReportParams p,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetShiftVarianceReportQuery(p.BranchId, p.CashierId, p.DateFrom, p.DateTo), ct);

        return TypedResults.Ok<object>(result);
    }

    // ── Request / Response records ────────────────────────────────────────────

    private sealed record OpenShiftRequest(string BranchId, decimal OpeningCashFund);
    private sealed record OpenShiftResponse(string Id);

    private sealed record RecordCashMovementRequest(
        CashMovementType Type,
        decimal Amount,
        string Reason,
        string? Reference);
    private sealed record RecordCashMovementResponse(string Id);

    private sealed record CloseShiftRequest(IReadOnlyList<DenominationEntryRequest> Denominations);
    private sealed record DenominationEntryRequest(decimal DenominationValue, int Count);

    private sealed record ReviewShiftRequest(ReviewStatus NewReviewStatus, string? Notes);
    private sealed record VoidShiftRequest(string Reason);

    private sealed class GetCurrentShiftParams
    {
        public string? BranchId { get; set; }
    }

    private sealed class GetShiftsParams
    {
        public string? BranchId { get; set; }
        public string? CashierId { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public ShiftStatus? Status { get; set; }
        public ReviewStatus? ReviewStatus { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    private sealed class GetVarianceReportParams
    {
        public string? BranchId { get; set; }
        public string? CashierId { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
    }
}
