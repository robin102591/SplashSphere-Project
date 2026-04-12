using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.PricingModifiers.Commands.CreatePricingModifier;
using SplashSphere.Application.Features.PricingModifiers.Commands.DeletePricingModifier;
using SplashSphere.Application.Features.PricingModifiers.Commands.TogglePricingModifierStatus;
using SplashSphere.Application.Features.PricingModifiers.Commands.UpdatePricingModifier;
using SplashSphere.Application.Features.PricingModifiers.Queries.GetPricingModifierById;
using SplashSphere.Application.Features.PricingModifiers.Queries.GetPricingModifiers;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class PricingModifierEndpoints
{
    public static IEndpointRouteBuilder MapPricingModifierEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/pricing-modifiers")
            .WithTags("PricingModifiers")
            .RequireAuthorization()
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.PricingModifiers));

        group.MapGet("/",          GetAll)         .WithName("GetPricingModifiers").WithSummary("List pricing modifiers");
        group.MapGet("/{id}",      GetById)        .WithName("GetPricingModifierById").WithSummary("Get modifier by ID");
        group.MapPost("/",         Create)         .WithName("CreatePricingModifier").WithSummary("Create pricing modifier");
        group.MapPut("/{id}",      Update)         .WithName("UpdatePricingModifier").WithSummary("Update pricing modifier");
        group.MapDelete("/{id}",   Delete)         .WithName("DeletePricingModifier").WithSummary("Delete pricing modifier");
        group.MapPatch("/{id}/toggle", Toggle)     .WithName("TogglePricingModifierStatus").WithSummary("Toggle modifier status");

        return app;
    }

    // ── GET /api/v1/pricing-modifiers?branchId=&type=&activeOnly=true ────────

    private static async Task<IResult> GetAll(
        [AsParameters] GetPricingModifiersQuery query,
        ISender sender,
        CancellationToken ct)
        => TypedResults.Ok(await sender.Send(query, ct));

    // ── GET /api/v1/pricing-modifiers/{id} ───────────────────────────────────

    private static async Task<IResult> GetById(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var dto = await sender.Send(new GetPricingModifierByIdQuery(id), ct);
        return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
    }

    // ── POST /api/v1/pricing-modifiers ───────────────────────────────────────

    private static async Task<IResult> Create(
        CreatePricingModifierCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/pricing-modifiers/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    // ── PUT /api/v1/pricing-modifiers/{id} ───────────────────────────────────

    private static async Task<IResult> Update(
        string id,
        UpdatePricingModifierBody body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdatePricingModifierCommand(
            id,
            body.Name,
            body.Type,
            body.Value,
            body.BranchId,
            body.StartTime,
            body.EndTime,
            body.ActiveDayOfWeek,
            body.HolidayDate,
            body.HolidayName,
            body.StartDate,
            body.EndDate), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── DELETE /api/v1/pricing-modifiers/{id} ────────────────────────────────

    private static async Task<IResult> Delete(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeletePricingModifierCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PATCH /api/v1/pricing-modifiers/{id}/toggle ──────────────────────────

    private static async Task<IResult> Toggle(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new TogglePricingModifierStatusCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private sealed record UpdatePricingModifierBody(
        string Name,
        ModifierType Type,
        decimal Value,
        string? BranchId,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        DayOfWeek? ActiveDayOfWeek,
        DateOnly? HolidayDate,
        string? HolidayName,
        DateOnly? StartDate,
        DateOnly? EndDate);
}
