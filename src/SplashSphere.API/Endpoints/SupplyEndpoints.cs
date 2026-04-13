using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.Inventory;
using SplashSphere.Application.Features.Inventory.Commands.CreateSupplyCategory;
using SplashSphere.Application.Features.Inventory.Commands.CreateSupplyItem;
using SplashSphere.Application.Features.Inventory.Commands.DeleteSupplyItem;
using SplashSphere.Application.Features.Inventory.Commands.UpdateSupplyItem;
using SplashSphere.Application.Features.Inventory.Queries.GetSupplies;
using SplashSphere.Application.Features.Inventory.Queries.GetSupplyById;
using SplashSphere.Application.Features.Inventory.Queries.GetSupplyCategories;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class SupplyEndpoints
{
    public static IEndpointRouteBuilder MapSupplyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/supplies")
            .RequireAuthorization()
            .WithTags("Inventory")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.SupplyTracking));

        group.MapGet("/", GetSupplies).WithSummary("List supply items with optional filters");
        group.MapPost("/", CreateSupplyItem).WithSummary("Create a new supply item");
        group.MapGet("/{id}", GetSupplyById).WithSummary("Get supply item detail with recent movements");
        group.MapPut("/{id}", UpdateSupplyItem).WithSummary("Update a supply item");
        group.MapDelete("/{id}", DeleteSupplyItem).WithSummary("Soft-delete a supply item");

        // Categories
        group.MapGet("/categories", GetSupplyCategories).WithSummary("List supply categories");
        group.MapPost("/categories", CreateSupplyCategory).WithSummary("Create a supply category");

        return app;
    }

    private static async Task<Ok<object>> GetSupplies(
        [AsParameters] SupplyListParams p, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new GetSuppliesQuery(p.CategoryId, p.BranchId, p.StockStatus, p.Page, p.PageSize), ct);
        return TypedResults.Ok<object>(result);
    }

    private static async Task<Results<Created<object>, BadRequest<ProblemDetails>>> CreateSupplyItem(
        [FromBody] CreateSupplyItemRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreateSupplyItemCommand(
            body.BranchId, body.Name, body.Unit, body.CategoryId,
            body.Description, body.ReorderLevel, body.InitialStock, body.InitialUnitCost), ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

        return TypedResults.Created($"/api/v1/supplies/{result.Value}", (object)new { id = result.Value });
    }

    private static async Task<Results<Ok<object>, NotFound>> GetSupplyById(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetSupplyByIdQuery(id), ct);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok<object>(result);
    }

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> UpdateSupplyItem(
        string id, [FromBody] UpdateSupplyItemRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateSupplyItemCommand(
            id, body.Name, body.Unit, body.CategoryId,
            body.Description, body.ReorderLevel, body.IsActive), ct);

        if (result.IsFailure)
            return result.Error.Code == "NotFound" ? TypedResults.NotFound()
                : TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteSupplyItem(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteSupplyItemCommand(id), ct);
        return result.IsFailure ? TypedResults.NotFound() : TypedResults.NoContent();
    }

    private static async Task<Ok<object>> GetSupplyCategories(ISender sender, CancellationToken ct)
        => TypedResults.Ok<object>(await sender.Send(new GetSupplyCategoriesQuery(), ct));

    private static async Task<Results<Created<object>, BadRequest<ProblemDetails>>> CreateSupplyCategory(
        [FromBody] CreateCategoryRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreateSupplyCategoryCommand(body.Name, body.Description), ct);
        if (result.IsFailure)
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        return TypedResults.Created($"/api/v1/supplies/categories/{result.Value}", (object)new { id = result.Value });
    }

    // Request records
    private sealed record CreateSupplyItemRequest(
        string BranchId, string Name, string Unit,
        string? CategoryId = null, string? Description = null,
        decimal? ReorderLevel = null, decimal InitialStock = 0, decimal InitialUnitCost = 0);

    private sealed record UpdateSupplyItemRequest(
        string Name, string Unit,
        string? CategoryId = null, string? Description = null,
        decimal? ReorderLevel = null, bool IsActive = true);

    private sealed record CreateCategoryRequest(string Name, string? Description = null);

    private sealed record SupplyListParams(
        string? CategoryId = null, string? BranchId = null,
        string? StockStatus = null, int Page = 1, int PageSize = 50);
}
