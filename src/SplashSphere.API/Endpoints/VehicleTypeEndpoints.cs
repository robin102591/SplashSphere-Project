using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.VehicleTypes.Commands.CreateVehicleType;
using SplashSphere.Application.Features.VehicleTypes.Commands.ToggleVehicleTypeStatus;
using SplashSphere.Application.Features.VehicleTypes.Commands.UpdateVehicleType;
using SplashSphere.Application.Features.VehicleTypes.Queries.GetVehicleTypeById;
using SplashSphere.Application.Features.VehicleTypes.Queries.GetVehicleTypes;

namespace SplashSphere.API.Endpoints;

public static class VehicleTypeEndpoints
{
    public static IEndpointRouteBuilder MapVehicleTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/vehicle-types")
            .WithTags("VehicleTypes")
            .RequireAuthorization();

        group.MapGet("/",     GetVehicleTypes)  .WithName("GetVehicleTypes");
        group.MapGet("/{id}", GetVehicleTypeById).WithName("GetVehicleTypeById");
        group.MapPost("/",    CreateVehicleType) .WithName("CreateVehicleType");
        group.MapPut("/{id}", UpdateVehicleType) .WithName("UpdateVehicleType");
        group.MapPatch("/{id}/status", ToggleVehicleTypeStatus).WithName("ToggleVehicleTypeStatus");

        return app;
    }

    private static async Task<IResult> GetVehicleTypes(
        [AsParameters] GetVehicleTypesQuery query, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    private static async Task<IResult> GetVehicleTypeById(
        string id, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetVehicleTypeByIdQuery(id), ct));

    private static async Task<IResult> CreateVehicleType(
        CreateVehicleTypeCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/vehicle-types/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> UpdateVehicleType(
        string id, UpdateVehicleTypeRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateVehicleTypeCommand(id, body.Name), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> ToggleVehicleTypeStatus(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ToggleVehicleTypeStatusCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private sealed record UpdateVehicleTypeRequest(string Name);
}
