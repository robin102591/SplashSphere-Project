using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.Inventory;
using SplashSphere.Application.Features.Inventory.Commands.RecordBulkUsage;
using SplashSphere.Application.Features.Inventory.Commands.RecordStockMovement;
using SplashSphere.Application.Features.Inventory.Queries.GetStockMovements;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class StockMovementEndpoints
{
    public static IEndpointRouteBuilder MapStockMovementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/stock-movements")
            .RequireAuthorization()
            .WithTags("Inventory")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.SupplyTracking));

        group.MapPost("/", RecordStockMovement).WithSummary("Record a stock movement for a supply item");
        group.MapGet("/", GetStockMovements).WithSummary("List stock movements with optional filters");
        group.MapPost("/bulk-usage", RecordBulkUsage).WithSummary("Record bulk usage deductions for multiple supply items");

        return app;
    }

    private static async Task<Results<Created<object>, BadRequest<ProblemDetails>>> RecordStockMovement(
        [FromBody] RecordStockMovementRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RecordStockMovementCommand(
            body.SupplyItemId, body.Type, body.Quantity,
            body.UnitCost, body.Reference, body.Notes), ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

        return TypedResults.Created($"/api/v1/stock-movements/{result.Value}", (object)new { id = result.Value });
    }

    private static async Task<IResult> GetStockMovements(
        [AsParameters] StockMovementListParams p, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetStockMovementsQuery(
            p.SupplyItemId, p.MerchandiseId, p.Type, p.BranchId, p.From, p.To, p.Page, p.PageSize), ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> RecordBulkUsage(
        [FromBody] RecordBulkUsageRequest body, ISender sender, CancellationToken ct)
    {
        var items = body.Items.Select(i => new BulkUsageItem(i.SupplyItemId, i.Quantity, i.Notes)).ToList();
        var result = await sender.Send(new RecordBulkUsageCommand(body.BranchId, items), ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

        return TypedResults.Ok(new { count = result.Value });
    }

    // Request records
    private sealed record RecordStockMovementRequest(
        string SupplyItemId, MovementType Type, decimal Quantity,
        decimal? UnitCost = null, string? Reference = null, string? Notes = null);

    private sealed record BulkUsageItemRequest(string SupplyItemId, decimal Quantity, string? Notes = null);

    private sealed record RecordBulkUsageRequest(string BranchId, IReadOnlyList<BulkUsageItemRequest> Items);

    private sealed record StockMovementListParams(
        string? SupplyItemId = null, string? MerchandiseId = null,
        MovementType? Type = null, string? BranchId = null,
        DateOnly? From = null, DateOnly? To = null,
        int Page = 1, int PageSize = 50);
}
