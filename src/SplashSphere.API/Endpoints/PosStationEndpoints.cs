using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.PosStations.Commands.CreatePosStation;
using SplashSphere.Application.Features.PosStations.Commands.DeletePosStation;
using SplashSphere.Application.Features.PosStations.Commands.UpdatePosStation;
using SplashSphere.Application.Features.PosStations.Queries.GetPosStationById;
using SplashSphere.Application.Features.PosStations.Queries.GetPosStations;

namespace SplashSphere.API.Endpoints;

public static class PosStationEndpoints
{
    public static IEndpointRouteBuilder MapPosStationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/branches/{branchId}/stations")
            .WithTags("POS Stations")
            .RequireAuthorization();

        group.MapGet("/",      GetStations)      .WithName("GetPosStations").WithSummary("List POS stations for a branch");
        group.MapGet("/{id}",  GetStationById)   .WithName("GetPosStationById").WithSummary("Get POS station by ID");
        group.MapPost("/",     CreateStation)    .WithName("CreatePosStation").WithSummary("Create a POS station");
        group.MapPut("/{id}",  UpdateStation)    .WithName("UpdatePosStation").WithSummary("Update a POS station");
        group.MapDelete("/{id}", DeleteStation)  .WithName("DeletePosStation").WithSummary("Delete a POS station");

        return app;
    }

    private static async Task<IResult> GetStations(
        string branchId,
        ISender sender,
        CancellationToken ct)
    {
        var stations = await sender.Send(new GetPosStationsQuery(branchId), ct);
        return TypedResults.Ok(stations);
    }

    private static async Task<IResult> GetStationById(
        string branchId,
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var dto = await sender.Send(new GetPosStationByIdQuery(id), ct);
        return TypedResults.Ok(dto);
    }

    private static async Task<IResult> CreateStation(
        string branchId,
        CreatePosStationRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreatePosStationCommand(branchId, body.Name), ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/branches/{branchId}/stations/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> UpdateStation(
        string branchId,
        string id,
        UpdatePosStationRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdatePosStationCommand(id, body.Name, body.IsActive), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> DeleteStation(
        string branchId,
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeletePosStationCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private sealed record CreatePosStationRequest(string Name);
    private sealed record UpdatePosStationRequest(string Name, bool IsActive);
}
