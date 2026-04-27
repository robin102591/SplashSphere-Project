using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Payroll.Commands.AddPayrollAdjustment;
using SplashSphere.Application.Features.Payroll.Commands.BulkApplyAdjustment;
using SplashSphere.Application.Features.Payroll.Commands.ClosePayrollPeriod;
using SplashSphere.Application.Features.Payroll.Commands.CreatePayrollPeriod;
using SplashSphere.Application.Features.Payroll.Commands.CreatePayrollTemplate;
using SplashSphere.Application.Features.Payroll.Commands.DeletePayrollAdjustment;
using SplashSphere.Application.Features.Payroll.Commands.DeletePayrollTemplate;
using SplashSphere.Application.Features.Payroll.Commands.ProcessPayrollPeriod;
using SplashSphere.Application.Features.Payroll.Commands.ReleasePayrollPeriod;
using SplashSphere.Application.Features.Payroll.Queries.ExportPayrollCsv;
using SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollAdjustment;
using SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollEntry;
using SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollTemplate;
using SplashSphere.Application.Features.Payroll.Queries.GetPayrollEntryDetail;
using SplashSphere.Application.Features.Payroll.Queries.GetPayrollPeriodById;
using SplashSphere.Application.Features.Payroll.Queries.GetPayrollPeriods;
using SplashSphere.Application.Features.Payroll.Queries.GetPayrollTemplates;
using SplashSphere.Application.Features.Payroll.Queries.GetPayslip;
using SplashSphere.Application.Features.Payroll.Queries.ExportPayslipPdf;
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
        group.MapGet("/periods",                          GetPayrollPeriods).WithSummary("List payroll periods");
        group.MapPost("/periods",                         CreatePayrollPeriod).WithSummary("Create payroll period");
        group.MapGet("/periods/{id}",                     GetPayrollPeriodById).WithSummary("Get period with entries");
        group.MapPost("/periods/{id}/close",              ClosePayrollPeriod).WithSummary("Close payroll period");
        group.MapPost("/periods/{id}/process",            ProcessPayrollPeriod).WithSummary("Process payroll period");
        group.MapPost("/periods/{id}/release",            ReleasePayrollPeriod).WithSummary("Release payroll");
        group.MapGet("/periods/{id}/export/csv",          ExportPayrollCsv).WithSummary("Export period as CSV");

        // ── Entries ───────────────────────────────────────────────────────────
        group.MapPatch("/entries/{id}",                   UpdatePayrollEntry).WithSummary("Update payroll entry");
        group.MapPost("/entries/bulk-adjust",              BulkApplyAdjustment).WithSummary("Bulk adjust entries");
        group.MapGet("/entries/{id}/detail",               GetPayrollEntryDetail).WithSummary("Get entry detail with breakdown");
        group.MapGet("/entries/{id}/payslip",              GetPayslip).WithSummary("Get payslip data");
        group.MapGet("/entries/{id}/payslip/pdf",          ExportPayslipPdf).WithSummary("Download payslip PDF");

        // ── Entry adjustments ───────────────────────────────────────────────
        group.MapPost("/entries/{id}/adjustments",         AddPayrollAdjustment).WithSummary("Add adjustment to entry");
        group.MapPut("/adjustments/{adjustmentId}",        UpdatePayrollAdjustment).WithSummary("Update adjustment");
        group.MapDelete("/adjustments/{adjustmentId}",     DeletePayrollAdjustment).WithSummary("Delete adjustment");

        // ── Adjustment templates ────────────────────────────────────────────
        group.MapGet("/templates",                        GetPayrollTemplates).WithSummary("List adjustment templates");
        group.MapPost("/templates",                       CreatePayrollTemplate).WithSummary("Create adjustment template");
        group.MapPut("/templates/{id}",                   UpdatePayrollTemplate).WithSummary("Update adjustment template");
        group.MapDelete("/templates/{id}",                DeletePayrollTemplate).WithSummary("Delete adjustment template");

        return app;
    }

    // ── GET /periods ──────────────────────────────────────────────────────────

    private static async Task<Ok<object>> GetPayrollPeriods(
        [AsParameters] GetPeriodsParams p,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetPayrollPeriodsQuery(p.Page, p.PageSize, p.Status, p.Year, p.BranchId), ct);
        return TypedResults.Ok<object>(result);
    }

    // ── POST /periods ──────────────────────────────────────────────────────────

    private static async Task<IResult> CreatePayrollPeriod(
        [FromBody] CreatePeriodRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreatePayrollPeriodCommand(body.StartDate, body.EndDate, body.BranchId), ct);

        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/payroll/periods/{result.Value}", new { id = result.Value })
            : result.ToProblem();
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

    private static async Task<IResult> ClosePayrollPeriod(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ClosePayrollPeriodCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── POST /periods/{id}/process ────────────────────────────────────────────

    private static async Task<IResult> ProcessPayrollPeriod(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ProcessPayrollPeriodCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── POST /periods/{id}/release ──────────────────────────────────────────────

    private static async Task<IResult> ReleasePayrollPeriod(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ReleasePayrollPeriodCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── GET /periods/{id}/export/csv ────────────────────────────────────────────

    private static async Task<IResult> ExportPayrollCsv(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ExportPayrollCsvQuery(id), ct);
        if (result is null) return TypedResults.NotFound();
        return TypedResults.File(result.Content, "text/csv", result.FileName);
    }

    // ── PATCH /entries/{id} ───────────────────────────────────────────────────

    private static async Task<IResult> UpdatePayrollEntry(
        string id,
        [FromBody] UpdateEntryRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdatePayrollEntryCommand(id, body.Notes), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
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

    // ── GET /entries/{id}/payslip ────────────────────────────────────────────

    private static async Task<Results<Ok<object>, NotFound>> GetPayslip(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetPayslipQuery(id), ct);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok<object>(result);
    }

    // ── GET /entries/{id}/payslip/pdf ──────────────────────────────────────

    private static async Task<IResult> ExportPayslipPdf(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new ExportPayslipPdfQuery(id), ct);
        if (result is null) return TypedResults.NotFound();
        return TypedResults.File(result.Content, "application/pdf", result.FileName);
    }

    // ── POST /entries/bulk-adjust ───────────────────────────────────────────

    private static async Task<IResult> BulkApplyAdjustment(
        [FromBody] BulkAdjustRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new BulkApplyAdjustmentCommand(body.EntryIds, body.AdjustmentType, body.Amount, body.Notes, body.TemplateId), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── POST /entries/{id}/adjustments ──────────────────────────────────────

    private static async Task<IResult> AddPayrollAdjustment(
        string id,
        [FromBody] AddAdjustmentRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new AddPayrollAdjustmentCommand(id, body.Type, body.Category, body.Amount, body.Notes, body.TemplateId), ct);

        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/payroll/adjustments/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    // ── PUT /adjustments/{adjustmentId} ─────────────────────────────────────

    private static async Task<IResult> UpdatePayrollAdjustment(
        string adjustmentId,
        [FromBody] UpdateAdjustmentRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdatePayrollAdjustmentCommand(adjustmentId, body.Amount, body.Notes), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── DELETE /adjustments/{adjustmentId} ──────────────────────────────────

    private static async Task<IResult> DeletePayrollAdjustment(
        string adjustmentId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new DeletePayrollAdjustmentCommand(adjustmentId), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
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

    private static async Task<IResult> CreatePayrollTemplate(
        [FromBody] CreateTemplateRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreatePayrollTemplateCommand(body.Name, body.Type, body.DefaultAmount), ct);

        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/payroll/templates/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    // ── PUT /templates/{id} ─────────────────────────────────────────────────

    private static async Task<IResult> UpdatePayrollTemplate(
        string id,
        [FromBody] UpdateTemplateRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdatePayrollTemplateCommand(id, body.Name, body.Type, body.DefaultAmount, body.SortOrder), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
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
        int? Year = null,
        string? BranchId = null);

    private sealed record CreatePeriodRequest(
        DateOnly StartDate,
        DateOnly EndDate,
        string? BranchId = null);

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
