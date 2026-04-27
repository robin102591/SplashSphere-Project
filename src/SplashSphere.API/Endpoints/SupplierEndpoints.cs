using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Inventory;
using SplashSphere.Application.Features.Inventory.Commands.CreateSupplier;
using SplashSphere.Application.Features.Inventory.Commands.UpdateSupplier;
using SplashSphere.Application.Features.Inventory.Queries.GetSuppliers;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class SupplierEndpoints
{
    public static IEndpointRouteBuilder MapSupplierEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/suppliers")
            .RequireAuthorization()
            .WithTags("Inventory")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.PurchaseOrders));

        group.MapGet("/", GetSuppliers).WithSummary("List all suppliers");
        group.MapPost("/", CreateSupplier).WithSummary("Create a supplier");
        group.MapPut("/{id}", UpdateSupplier).WithSummary("Update a supplier");

        return app;
    }

    private static async Task<IResult> GetSuppliers(ISender sender, CancellationToken ct)
        => TypedResults.Ok(await sender.Send(new GetSuppliersQuery(), ct));

    private static async Task<IResult> CreateSupplier(
        [FromBody] CreateSupplierRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreateSupplierCommand(
            body.Name, body.ContactPerson, body.Phone, body.Email, body.Address), ct);

        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/suppliers/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> UpdateSupplier(
        string id, [FromBody] UpdateSupplierRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateSupplierCommand(
            id, body.Name, body.ContactPerson, body.Phone, body.Email, body.Address, body.IsActive), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // Request records
    private sealed record CreateSupplierRequest(
        string Name, string? ContactPerson = null, string? Phone = null,
        string? Email = null, string? Address = null);

    private sealed record UpdateSupplierRequest(
        string Name, string? ContactPerson, string? Phone,
        string? Email, string? Address, bool IsActive);
}
