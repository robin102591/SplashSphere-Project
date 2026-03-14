using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Packages.Commands.CreatePackage;
using SplashSphere.Application.Features.Packages.Commands.TogglePackageStatus;
using SplashSphere.Application.Features.Packages.Commands.UpdatePackage;
using SplashSphere.Application.Features.Packages.Commands.UpsertPackageCommission;
using SplashSphere.Application.Features.Packages.Commands.UpsertPackagePricing;
using SplashSphere.Application.Features.Packages.Queries.GetPackageById;
using SplashSphere.Application.Features.Packages.Queries.GetPackages;

namespace SplashSphere.API.Endpoints;

public static class PackageEndpoints
{
    public static IEndpointRouteBuilder MapPackageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/packages")
            .WithTags("Packages")
            .RequireAuthorization();

        group.MapGet("/",                  GetPackages)         .WithName("GetPackages");
        group.MapGet("/{id}",              GetPackageById)      .WithName("GetPackageById");
        group.MapPost("/",                 CreatePackage)       .WithName("CreatePackage");
        group.MapPut("/{id}",              UpdatePackage)       .WithName("UpdatePackage");
        group.MapPatch("/{id}/status",     TogglePackageStatus) .WithName("TogglePackageStatus");
        group.MapPut("/{id}/pricing",      UpsertPricing)       .WithName("UpsertPackagePricing");
        group.MapPut("/{id}/commissions",  UpsertCommissions)   .WithName("UpsertPackageCommissions");

        return app;
    }

    // ── GET /api/v1/packages?search=&page=&pageSize= ─────────────────────────

    private static async Task<IResult> GetPackages(
        [AsParameters] GetPackagesQuery query, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    // ── GET /api/v1/packages/{id}  (includes services + full matrices) ────────

    private static async Task<IResult> GetPackageById(
        string id, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetPackageByIdQuery(id), ct));

    // ── POST /api/v1/packages ─────────────────────────────────────────────────

    private static async Task<IResult> CreatePackage(
        CreatePackageCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/packages/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    // ── PUT /api/v1/packages/{id} ─────────────────────────────────────────────

    private static async Task<IResult> UpdatePackage(
        string id, UpdatePackageRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdatePackageCommand(id, body.Name, body.Description, body.ServiceIds), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PATCH /api/v1/packages/{id}/status ───────────────────────────────────

    private static async Task<IResult> TogglePackageStatus(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new TogglePackageStatusCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PUT /api/v1/packages/{id}/pricing ─────────────────────────────────────
    // Body: { "rows": [{ "vehicleTypeId": "...", "sizeId": "...", "price": 0.00 }] }
    // Empty rows array clears the matrix.

    private static async Task<IResult> UpsertPricing(
        string id, UpsertPricingBody body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpsertPackagePricingCommand(id, body.Rows), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PUT /api/v1/packages/{id}/commissions ─────────────────────────────────
    // Body: { "rows": [{ "vehicleTypeId": "...", "sizeId": "...", "percentageRate": 15.00 }] }
    // Package commissions are always percentage-based. Empty rows clears the matrix.

    private static async Task<IResult> UpsertCommissions(
        string id, UpsertCommissionsBody body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpsertPackageCommissionCommand(id, body.Rows), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── Request body types ────────────────────────────────────────────────────

    private sealed record UpdatePackageRequest(
        string Name,
        string? Description,
        IReadOnlyList<string> ServiceIds);

    private sealed record UpsertPricingBody(
        IReadOnlyList<PackagePricingRowRequest> Rows);

    private sealed record UpsertCommissionsBody(
        IReadOnlyList<PackageCommissionRowRequest> Rows);
}
