using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Inventory;
using SplashSphere.Application.Features.Inventory.Commands.CancelPurchaseOrder;
using SplashSphere.Application.Features.Inventory.Commands.CreatePurchaseOrder;
using SplashSphere.Application.Features.Inventory.Commands.ReceivePurchaseOrder;
using SplashSphere.Application.Features.Inventory.Commands.SendPurchaseOrder;
using SplashSphere.Application.Features.Inventory.Commands.UpdatePurchaseOrder;
using SplashSphere.Application.Features.Inventory.Queries.GetPurchaseOrderById;
using SplashSphere.Application.Features.Inventory.Queries.GetPurchaseOrders;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class PurchaseOrderEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/purchase-orders")
            .RequireAuthorization()
            .WithTags("Inventory")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.PurchaseOrders));

        group.MapGet("/", GetPurchaseOrders).WithSummary("List purchase orders");
        group.MapPost("/", CreatePurchaseOrder).WithSummary("Create a purchase order");
        group.MapGet("/{id}", GetPurchaseOrderById).WithSummary("Get purchase order details");
        group.MapPut("/{id}", UpdatePurchaseOrder).WithSummary("Update a draft purchase order");
        group.MapPatch("/{id}/send", SendPurchaseOrder).WithSummary("Send a purchase order to supplier");
        group.MapPost("/{id}/receive", ReceivePurchaseOrder).WithSummary("Receive goods against a purchase order");
        group.MapPatch("/{id}/cancel", CancelPurchaseOrder).WithSummary("Cancel a purchase order");

        return app;
    }

    private static async Task<IResult> GetPurchaseOrders(
        [AsParameters] PurchaseOrderListParams p, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new GetPurchaseOrdersQuery(p.SupplierId, p.BranchId, p.Status, p.Page, p.PageSize), ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> CreatePurchaseOrder(
        [FromBody] CreatePurchaseOrderRequest body, ISender sender, CancellationToken ct)
    {
        var lines = body.Lines.Select(l =>
            new CreatePurchaseOrderLineRequest(l.SupplyItemId, l.MerchandiseId, l.ItemName, l.Quantity, l.UnitCost))
            .ToList();

        var result = await sender.Send(new CreatePurchaseOrderCommand(
            body.SupplierId, body.BranchId, body.Notes, body.ExpectedDeliveryDate, lines), ct);

        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/purchase-orders/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> GetPurchaseOrderById(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetPurchaseOrderByIdQuery(id), ct);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<IResult> UpdatePurchaseOrder(
        string id, [FromBody] UpdatePurchaseOrderRequest body, ISender sender, CancellationToken ct)
    {
        var lines = body.Lines.Select(l =>
            new CreatePurchaseOrderLineRequest(l.SupplyItemId, l.MerchandiseId, l.ItemName, l.Quantity, l.UnitCost))
            .ToList();

        var result = await sender.Send(new UpdatePurchaseOrderCommand(
            id, body.Notes, body.ExpectedDeliveryDate, lines), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> SendPurchaseOrder(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new SendPurchaseOrderCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> ReceivePurchaseOrder(
        string id, [FromBody] ReceivePurchaseOrderRequest body, ISender sender, CancellationToken ct)
    {
        var lines = body.Lines.Select(l => new ReceiveLineRequest(l.LineId, l.ReceivedQuantity)).ToList();
        var result = await sender.Send(new ReceivePurchaseOrderCommand(id, lines), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> CancelPurchaseOrder(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CancelPurchaseOrderCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // Request records
    private sealed record CreatePurchaseOrderRequest(
        string SupplierId, string BranchId, string? Notes,
        DateTime? ExpectedDeliveryDate, List<PurchaseOrderLineRequest> Lines);

    private sealed record UpdatePurchaseOrderRequest(
        string? Notes, DateTime? ExpectedDeliveryDate, List<PurchaseOrderLineRequest> Lines);

    private sealed record PurchaseOrderLineRequest(
        string? SupplyItemId, string? MerchandiseId, string ItemName, decimal Quantity, decimal UnitCost);

    private sealed record ReceivePurchaseOrderRequest(List<ReceiveLineRequestDto> Lines);

    private sealed record ReceiveLineRequestDto(string LineId, decimal ReceivedQuantity);

    private sealed record PurchaseOrderListParams(
        string? SupplierId = null, string? BranchId = null,
        PurchaseOrderStatus? Status = null, int Page = 1, int PageSize = 50);
}
