using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Merchandise.Commands.AdjustStock;
using SplashSphere.Application.Features.Merchandise.Commands.CreateMerchandise;
using SplashSphere.Application.Features.Merchandise.Commands.ToggleMerchandiseStatus;
using SplashSphere.Application.Features.Merchandise.Commands.UpdateMerchandise;
using SplashSphere.Application.Features.Merchandise.Queries.GetMerchandise;
using SplashSphere.Application.Features.Merchandise.Queries.GetLowStockMerchandise;
using SplashSphere.Application.Features.Merchandise.Queries.GetMerchandiseById;

namespace SplashSphere.API.Endpoints;

public static class MerchandiseEndpoints
{
    public static IEndpointRouteBuilder MapMerchandiseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/merchandise")
            .WithTags("Merchandise")
            .RequireAuthorization();

        group.MapGet("/",                      GetMerchandise)         .WithName("GetMerchandise").WithSummary("List merchandise");
        group.MapGet("/{id}",                  GetMerchandiseById)     .WithName("GetMerchandiseById").WithSummary("Get merchandise by ID");
        group.MapPost("/",                     CreateMerchandise)      .WithName("CreateMerchandise").WithSummary("Create merchandise");
        group.MapPut("/{id}",                  UpdateMerchandise)      .WithName("UpdateMerchandise").WithSummary("Update merchandise");
        group.MapPatch("/{id}/status",         ToggleMerchandiseStatus).WithName("ToggleMerchandiseStatus").WithSummary("Toggle merchandise active status");
        group.MapPost("/{id}/stock-adjustment",AdjustStock)            .WithName("AdjustStock").WithSummary("Adjust stock quantity").WithDescription("Positive values add stock, negative values remove stock.");
        group.MapGet("/low-stock",             GetLowStock)            .WithName("GetLowStockMerchandise").WithSummary("List low-stock items");

        return app;
    }

    // ── GET /api/v1/merchandise/low-stock

    private static async Task<IResult> GetLowStock(
        ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetLowStockMerchandiseQuery(), ct));

    // ── GET /api/v1/merchandise?categoryId=&lowStockOnly=&search=&page=&pageSize=

    private static async Task<IResult> GetMerchandise(
        [AsParameters] GetMerchandiseQuery query, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    // ── GET /api/v1/merchandise/{id} ──────────────────────────────────────────

    private static async Task<IResult> GetMerchandiseById(
        string id, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetMerchandiseByIdQuery(id), ct));

    // ── POST /api/v1/merchandise ──────────────────────────────────────────────

    private static async Task<IResult> CreateMerchandise(
        CreateMerchandiseCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/merchandise/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    // ── PUT /api/v1/merchandise/{id} ──────────────────────────────────────────
    // SKU and StockQuantity are NOT in the update body — SKU is immutable,
    // stock is managed exclusively via stock-adjustment.

    private static async Task<IResult> UpdateMerchandise(
        string id, UpdateMerchandiseRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateMerchandiseCommand(
                id, body.Name, body.Price, body.LowStockThreshold,
                body.CategoryId, body.Description, body.CostPrice),
            ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PATCH /api/v1/merchandise/{id}/status ─────────────────────────────────

    private static async Task<IResult> ToggleMerchandiseStatus(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ToggleMerchandiseStatusCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── POST /api/v1/merchandise/{id}/stock-adjustment ────────────────────────
    // Body: { "adjustment": 50, "reason": "Restock received" }
    //       { "adjustment": -3, "reason": "Damaged — write-off" }
    // Returns 200 + updated stock quantities on success.

    private static async Task<IResult> AdjustStock(
        string id, AdjustStockBody body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new AdjustStockCommand(id, body.Adjustment, body.Reason), ct);

        if (!result.IsSuccess)
            return result.ToProblem();

        // Re-fetch the updated quantities so the caller sees the new state.
        var dto = await sender.Send(new GetMerchandiseByIdQuery(id), ct);
        return TypedResults.Ok(new { dto.StockQuantity, dto.IsLowStock });
    }

    // ── Request body types ────────────────────────────────────────────────────

    private sealed record UpdateMerchandiseRequest(
        string Name,
        decimal Price,
        int LowStockThreshold,
        string? CategoryId,
        string? Description,
        decimal? CostPrice);

    private sealed record AdjustStockBody(
        int Adjustment,
        string? Reason);
}
