using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Inventory;
using SplashSphere.Application.Features.Inventory.Commands.LogMaintenance;
using SplashSphere.Application.Features.Inventory.Commands.RegisterEquipment;
using SplashSphere.Application.Features.Inventory.Commands.UpdateEquipment;
using SplashSphere.Application.Features.Inventory.Commands.UpdateEquipmentStatus;
using SplashSphere.Application.Features.Inventory.Queries.GetEquipment;
using SplashSphere.Application.Features.Inventory.Queries.GetEquipmentById;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class EquipmentEndpoints
{
    public static IEndpointRouteBuilder MapEquipmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/equipment")
            .RequireAuthorization()
            .WithTags("Inventory")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.EquipmentManagement));

        group.MapGet("/", GetEquipment).WithSummary("List equipment");
        group.MapPost("/", RegisterEquipment).WithSummary("Register new equipment");
        group.MapGet("/{id}", GetEquipmentById).WithSummary("Get equipment details");
        group.MapPut("/{id}", UpdateEquipment).WithSummary("Update equipment");
        group.MapPost("/{id}/maintenance", LogMaintenance).WithSummary("Log a maintenance activity");
        group.MapPatch("/{id}/status", UpdateEquipmentStatus).WithSummary("Update equipment status");

        return app;
    }

    private static async Task<IResult> GetEquipment(
        [AsParameters] EquipmentListParams p, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new GetEquipmentQuery(p.BranchId, p.Status, p.Page, p.PageSize), ct);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> RegisterEquipment(
        [FromBody] RegisterEquipmentRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RegisterEquipmentCommand(
            body.BranchId, body.Name, body.Brand, body.Model, body.SerialNumber,
            body.PurchaseDate, body.PurchaseCost, body.WarrantyExpiry, body.Location, body.Notes), ct);

        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/equipment/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> GetEquipmentById(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetEquipmentByIdQuery(id), ct);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<IResult> UpdateEquipment(
        string id, [FromBody] UpdateEquipmentRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateEquipmentCommand(
            id, body.Name, body.Brand, body.Model, body.SerialNumber,
            body.PurchaseDate, body.PurchaseCost, body.WarrantyExpiry,
            body.Location, body.Notes, body.IsActive), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> LogMaintenance(
        string id, [FromBody] LogMaintenanceRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new LogMaintenanceCommand(
            id, body.Type, body.Description, body.Cost, body.PerformedBy,
            body.PerformedDate, body.NextDueDate, body.NextDueHours, body.Notes), ct);

        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/equipment/{id}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> UpdateEquipmentStatus(
        string id, [FromBody] UpdateEquipmentStatusRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateEquipmentStatusCommand(id, body.Status), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // Request records
    private sealed record RegisterEquipmentRequest(
        string BranchId, string Name, string? Brand = null, string? Model = null,
        string? SerialNumber = null, DateTime? PurchaseDate = null, decimal? PurchaseCost = null,
        DateTime? WarrantyExpiry = null, string? Location = null, string? Notes = null);

    private sealed record UpdateEquipmentRequest(
        string Name, string? Brand, string? Model, string? SerialNumber,
        DateTime? PurchaseDate, decimal? PurchaseCost, DateTime? WarrantyExpiry,
        string? Location, string? Notes, bool IsActive);

    private sealed record LogMaintenanceRequest(
        MaintenanceType Type, string Description, decimal? Cost = null,
        string? PerformedBy = null, DateTime PerformedDate = default,
        DateTime? NextDueDate = null, int? NextDueHours = null, string? Notes = null);

    private sealed record UpdateEquipmentStatusRequest(EquipmentStatus Status);

    private sealed record EquipmentListParams(
        string? BranchId = null, EquipmentStatus? Status = null,
        int Page = 1, int PageSize = 50);
}
