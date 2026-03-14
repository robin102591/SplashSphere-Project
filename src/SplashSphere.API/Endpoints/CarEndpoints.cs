using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Cars.Commands.CreateCar;
using SplashSphere.Application.Features.Cars.Commands.UpdateCar;
using SplashSphere.Application.Features.Cars.Queries.GetCarById;
using SplashSphere.Application.Features.Cars.Queries.GetCars;
using SplashSphere.Application.Features.Cars.Queries.LookupCarByPlate;

namespace SplashSphere.API.Endpoints;

public static class CarEndpoints
{
    public static IEndpointRouteBuilder MapCarEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/cars")
            .WithTags("Cars")
            .RequireAuthorization();

        // Fast POS plate lookup — must be registered BEFORE "/{id}" to avoid route ambiguity.
        group.MapGet("/lookup/{plateNumber}", LookupByPlate) .WithName("LookupCarByPlate");

        group.MapGet("/",       GetCars)      .WithName("GetCars");
        group.MapGet("/{id}",   GetCarById)   .WithName("GetCarById");
        group.MapPost("/",      CreateCar)    .WithName("CreateCar");
        group.MapPut("/{id}",   UpdateCar)    .WithName("UpdateCar");

        return app;
    }

    // ── GET /api/v1/cars/lookup/{plateNumber} ─────────────────────────────────
    // POS fast path — plate is normalised server-side (uppercase + trim).
    // Returns 200 + null body when plate is not found; cashier proceeds to create the car.

    private static async Task<IResult> LookupByPlate(
        string plateNumber, ISender sender, CancellationToken ct)
    {
        var dto = await sender.Send(new LookupCarByPlateQuery(plateNumber), ct);
        return TypedResults.Ok(dto);
    }

    // ── GET /api/v1/cars?customerId=&search=&page=&pageSize= ─────────────────

    private static async Task<IResult> GetCars(
        [AsParameters] GetCarsQuery query, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    // ── GET /api/v1/cars/{id} ─────────────────────────────────────────────────

    private static async Task<IResult> GetCarById(
        string id, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetCarByIdQuery(id), ct));

    // ── POST /api/v1/cars ─────────────────────────────────────────────────────

    private static async Task<IResult> CreateCar(
        CreateCarCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/cars/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    // ── PUT /api/v1/cars/{id} ─────────────────────────────────────────────────
    // PlateNumber and CustomerId are immutable — not accepted in the update body.

    private static async Task<IResult> UpdateCar(
        string id, UpdateCarRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateCarCommand(id, body.VehicleTypeId, body.SizeId, body.MakeId, body.ModelId, body.Color, body.Year, body.Notes),
            ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── Request body types ────────────────────────────────────────────────────

    private sealed record UpdateCarRequest(
        string VehicleTypeId,
        string SizeId,
        string? MakeId,
        string? ModelId,
        string? Color,
        int? Year,
        string? Notes);
}
