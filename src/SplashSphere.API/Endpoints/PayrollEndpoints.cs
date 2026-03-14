using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.Payroll.Commands.ClosePayrollPeriod;
using SplashSphere.Application.Features.Payroll.Commands.ProcessPayrollPeriod;
using SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollEntry;
using SplashSphere.Application.Features.Payroll.Queries.GetPayrollPeriodById;
using SplashSphere.Application.Features.Payroll.Queries.GetPayrollPeriods;
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
        group.MapGet("/periods/{id}",                     GetPayrollPeriodById);
        group.MapPost("/periods/{id}/close",              ClosePayrollPeriod);
        group.MapPost("/periods/{id}/process",            ProcessPayrollPeriod);

        // ── Entries ───────────────────────────────────────────────────────────
        group.MapPatch("/entries/{id}",                   UpdatePayrollEntry);

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
            new UpdatePayrollEntryCommand(id, body.Bonuses, body.Deductions, body.Notes), ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NotFound")
                return TypedResults.NotFound();
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.NoContent();
    }

    // ── Request / query-param records ─────────────────────────────────────────

    private sealed record GetPeriodsParams(
        int Page = 1,
        int PageSize = 20,
        PayrollStatus? Status = null,
        int? Year = null);

    private sealed record UpdateEntryRequest(
        decimal Bonuses,
        decimal Deductions,
        string? Notes);
}
