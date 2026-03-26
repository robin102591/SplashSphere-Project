using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.Payroll.Commands.AddPayrollAdjustment;
using SplashSphere.Application.Features.Payroll.Commands.BulkApplyAdjustment;
using SplashSphere.Application.Features.Payroll.Commands.ClosePayrollPeriod;
using SplashSphere.Application.Features.Payroll.Commands.CreatePayrollPeriod;
using SplashSphere.Application.Features.Payroll.Commands.CreatePayrollTemplate;
using SplashSphere.Application.Features.Payroll.Commands.DeletePayrollAdjustment;
using SplashSphere.Application.Features.Payroll.Commands.DeletePayrollTemplate;
using SplashSphere.Application.Features.Payroll.Commands.ProcessPayrollPeriod;
using SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollAdjustment;
using SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollEntry;
using SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollTemplate;
using SplashSphere.Application.Features.Payroll.Queries.GetPayrollEntryDetail;
using SplashSphere.Application.Features.Payroll.Queries.GetPayrollPeriodById;
using SplashSphere.Application.Features.Payroll.Queries.GetPayrollPeriods;
using SplashSphere.Application.Features.Payroll.Queries.GetPayrollTemplates;
using SplashSphere.Domain.Enums;

namespace SplashSphere.API.Endpoints;

public static class PayrollEndpoints
{
    public static IEndpointRouteBuilder MapPayrollEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/payroll")
            .RequireAuthorization()
            .WithTags("Payroll");

        // ── Periods ───────────────────────────────────────────────────────────
        group.MapGet("/periods",                          GetPayrollPeriods);
        group.MapPost("/periods",                         CreatePayrollPeriod);
        group.MapGet("/periods/{id}",                     GetPayrollPeriodById);
        group.MapPost("/periods/{id}/close",              ClosePayrollPeriod);
        group.MapPost("/periods/{id}/process",            ProcessPayrollPeriod);

        // ── Entries ───────────────────────────────────────────────────────────
        group.MapPatch("/entries/{id}",                   UpdatePayrollEntry);
        group.MapPost("/entries/bulk-adjust",              BulkApplyAdjustment);
        group.MapGet("/entries/{id}/detail",               GetPayrollEntryDetail);

        // ── Entry adjustments ───────────────────────────────────────────────
        group.MapPost("/entries/{id}/adjustments",         AddPayrollAdjustment);
        group.MapPut("/adjustments/{adjustmentId}",        UpdatePayrollAdjustment);
        group.MapDelete("/adjustments/{adjustmentId}",     DeletePayrollAdjustment);

        // ── Adjustment templates ────────────────────────────────────────────
        group.MapGet("/templates",                        GetPayrollTemplates);
        group.MapPost("/templates",                       CreatePayrollTemplate);
        group.MapPut("/templates/{id}",                   UpdatePayrollTemplate);
        group.MapDelete("/templates/{id}",                DeletePayrollTemplate);

        return app;
    }

    // ── GET /periods ──────────────────────────────────────────────────────────

    private static async Task<Ok<object>> GetPayrollPeriods(
        [AsParameters] GetPeriodsParams p,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetPayrollPeriodsQuery(p.Page, p.PageSize, p.Status, p.Year), ct);
        return TypedResults.Ok<object>(result);
    }

    // ── POST /periods ──────────────────────────────────────────────────────────

    private static async Task<Results<Created<object>, BadRequest<ProblemDetails>>> CreatePayrollPeriod(
        [FromBody] CreatePeriodRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreatePayrollPeriodCommand(body.StartDate, body.EndDate), ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

        return TypedResults.Created($"/api/v1/payroll/periods/{result.Value}", (object)new { id = result.Value });
    }

    // ── GET /periods/{id} ─────────────────────────────────────────────────────

    private static async Task<Results<Ok<object>, NotFound>> GetPayrollPeriodById(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetPayrollPeriodByIdQuery(id), ct);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok<object>(result);
    }

    // ── POST /periods/{id}/close ──────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> ClosePayrollPeriod(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ClosePayrollPeriodCommand(id), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── POST /periods/{id}/process ────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> ProcessPayrollPeriod(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ProcessPayrollPeriodCommand(id), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── PATCH /entries/{id} ───────────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> UpdatePayrollEntry(
        string id,
        [FromBody] UpdateEntryRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdatePayrollEntryCommand(id, body.Notes), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── GET /entries/{id}/detail ────────────────────────────────────────────

    private static async Task<Results<Ok<object>, NotFound>> GetPayrollEntryDetail(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetPayrollEntryDetailQuery(id), ct);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok<object>(result);
    }

    // ── POST /entries/bulk-adjust ───────────────────────────────────────────

    private static async Task<Results<NoContent, BadRequest<ProblemDetails>>> BulkApplyAdjustment(
        [FromBody] BulkAdjustRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new BulkApplyAdjustmentCommand(body.EntryIds, body.AdjustmentType, body.Amount, body.Notes, body.TemplateId), ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

        return TypedResults.NoContent();
    }

    // ── POST /entries/{id}/adjustments ──────────────────────────────────────

    private static async Task<Results<Created<object>, NotFound, BadRequest<ProblemDetails>>> AddPayrollAdjustment(
        string id,
        [FromBody] AddAdjustmentRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new AddPayrollAdjustmentCommand(id, body.Type, body.Category, body.Amount, body.Notes, body.TemplateId), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.Created($"/api/v1/payroll/adjustments/{result.Value}", (object)new { id = result.Value });
    }

    // ── PUT /adjustments/{adjustmentId} ─────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> UpdatePayrollAdjustment(
        string adjustmentId,
        [FromBody] UpdateAdjustmentRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdatePayrollAdjustmentCommand(adjustmentId, body.Amount, body.Notes), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── DELETE /adjustments/{adjustmentId} ──────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> DeletePayrollAdjustment(
        string adjustmentId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new DeletePayrollAdjustmentCommand(adjustmentId), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── GET /templates ──────────────────────────────────────────────────────

    private static async Task<Ok<object>> GetPayrollTemplates(
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetPayrollTemplatesQuery(), ct);
        return TypedResults.Ok<object>(result);
    }

    // ── POST /templates ─────────────────────────────────────────────────────

    private static async Task<Results<Created<object>, BadRequest<ProblemDetails>>> CreatePayrollTemplate(
        [FromBody] CreateTemplateRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreatePayrollTemplateCommand(body.Name, body.Type, body.DefaultAmount), ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

        return TypedResults.Created($"/api/v1/payroll/templates/{result.Value}", (object)new { id = result.Value });
    }

    // ── PUT /templates/{id} ─────────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> UpdatePayrollTemplate(
        string id,
        [FromBody] UpdateTemplateRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdatePayrollTemplateCommand(id, body.Name, body.Type, body.DefaultAmount, body.SortOrder), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── DELETE /templates/{id} ───────────────────────────────────────────────

    private static async Task<Results<NoContent, NotFound>> DeletePayrollTemplate(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeletePayrollTemplateCommand(id), ct);

        if (result.IsFailure)
            return TypedResults.NotFound();

        return TypedResults.NoContent();
    }

    // ── Request / query-param records ─────────────────────────────────────────

    private sealed record GetPeriodsParams(
        int Page = 1,
        int PageSize = 20,
        PayrollStatus? Status = null,
        int? Year = null);

    private sealed record CreatePeriodRequest(
        DateOnly StartDate,
        DateOnly EndDate);

    private sealed record UpdateEntryRequest(
        string? Notes);

    private sealed record BulkAdjustRequest(
        IReadOnlyList<string> EntryIds,
        AdjustmentType AdjustmentType,
        decimal Amount,
        string? Notes,
        string? TemplateId = null);

    private sealed record AddAdjustmentRequest(
        AdjustmentType Type,
        string Category,
        decimal Amount,
        string? Notes,
        string? TemplateId);

    private sealed record UpdateAdjustmentRequest(
        decimal Amount,
        string? Notes);

    private sealed record CreateTemplateRequest(
        string Name,
        AdjustmentType Type,
        decimal DefaultAmount);

    private sealed record UpdateTemplateRequest(
        string Name,
        AdjustmentType Type,
        decimal DefaultAmount,
        int SortOrder);
}
