using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Services.Commands.CreateService;
using SplashSphere.Application.Features.Services.Commands.ToggleServiceStatus;
using SplashSphere.Application.Features.Services.Commands.UpdateService;
using SplashSphere.Application.Features.Services.Commands.UpsertServiceCommission;
using SplashSphere.Application.Features.Services.Commands.UpsertServicePricing;
using SplashSphere.Application.Features.Services.Queries.GetServiceById;
using SplashSphere.Application.Features.Services.Queries.GetServices;

namespace SplashSphere.API.Endpoints;

public static class ServiceEndpoints
{
    public static IEndpointRouteBuilder MapServiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/services")
            .WithTags("Services")
            .RequireAuthorization();

        group.MapGet("/",                      GetServices)         .WithName("GetServices");
        group.MapGet("/{id}",                  GetServiceById)      .WithName("GetServiceById");
        group.MapPost("/",                     CreateService)       .WithName("CreateService");
        group.MapPut("/{id}",                  UpdateService)       .WithName("UpdateService");
        group.MapPatch("/{id}/status",         ToggleServiceStatus) .WithName("ToggleServiceStatus");
        group.MapPut("/{id}/pricing",          UpsertPricing)       .WithName("UpsertServicePricing");
        group.MapPut("/{id}/commissions",      UpsertCommissions)   .WithName("UpsertServiceCommissions");

        return app;
    }

    // ── GET /api/v1/services?categoryId=&search=&page=&pageSize= ────────────

    private static async Task<IResult> GetServices(
        [AsParameters] GetServicesQuery query, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    // ── GET /api/v1/services/{id}  (includes full pricing + commission matrices)

    private static async Task<IResult> GetServiceById(
        string id, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetServiceByIdQuery(id), ct));

    // ── POST /api/v1/services ────────────────────────────────────────────────

    private static async Task<IResult> CreateService(
        CreateServiceCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/services/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    // ── PUT /api/v1/services/{id} ────────────────────────────────────────────

    private static async Task<IResult> UpdateService(
        string id, UpdateServiceRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateServiceCommand(id, body.CategoryId, body.Name, body.BasePrice, body.Description),
            ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PATCH /api/v1/services/{id}/status ──────────────────────────────────

    private static async Task<IResult> ToggleServiceStatus(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ToggleServiceStatusCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PUT /api/v1/services/{id}/pricing ───────────────────────────────────
    // Body: { "rows": [{ "vehicleTypeId": "...", "sizeId": "...", "price": 0.00 }] }
    // Empty rows array clears the matrix (service reverts to BasePrice for all lookups).

    private static async Task<IResult> UpsertPricing(
        string id, UpsertPricingBody body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpsertServicePricingCommand(id, body.Rows), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PUT /api/v1/services/{id}/commissions ────────────────────────────────
    // Body: { "rows": [{ "vehicleTypeId": "...", "sizeId": "...", "type": 1,
    //                    "fixedAmount": null, "percentageRate": 15.00 }] }
    // Empty rows array clears the matrix (₱0 commission for all lookups).

    private static async Task<IResult> UpsertCommissions(
        string id, UpsertCommissionsBody body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpsertServiceCommissionCommand(id, body.Rows), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── Request body types ───────────────────────────────────────────────────

    private sealed record UpdateServiceRequest(
        string CategoryId,
        string Name,
        decimal BasePrice,
        string? Description);

    private sealed record UpsertPricingBody(
        IReadOnlyList<ServicePricingRowRequest> Rows);

    private sealed record UpsertCommissionsBody(
        IReadOnlyList<ServiceCommissionRowRequest> Rows);
}
