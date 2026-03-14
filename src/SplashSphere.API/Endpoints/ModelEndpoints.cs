using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Models.Commands.CreateModel;
using SplashSphere.Application.Features.Models.Commands.ToggleModelStatus;
using SplashSphere.Application.Features.Models.Commands.UpdateModel;
using SplashSphere.Application.Features.Models.Queries.GetModelById;
using SplashSphere.Application.Features.Models.Queries.GetModels;

namespace SplashSphere.API.Endpoints;

public static class ModelEndpoints
{
    public static IEndpointRouteBuilder MapModelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/models")
            .WithTags("Models")
            .RequireAuthorization();

        group.MapGet("/",     GetModels)  .WithName("GetModels");
        group.MapGet("/{id}", GetModelById).WithName("GetModelById");
        group.MapPost("/",    CreateModel) .WithName("CreateModel");
        group.MapPut("/{id}", UpdateModel) .WithName("UpdateModel");
        group.MapPatch("/{id}/status", ToggleModelStatus).WithName("ToggleModelStatus");

        return app;
    }

    private static async Task<IResult> GetModels(
        [AsParameters] GetModelsQuery query, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    private static async Task<IResult> GetModelById(
        string id, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetModelByIdQuery(id), ct));

    private static async Task<IResult> CreateModel(
        CreateModelCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/models/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> UpdateModel(
        string id, UpdateModelRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateModelCommand(id, body.Name), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> ToggleModelStatus(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ToggleModelStatusCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private sealed record UpdateModelRequest(string Name);
}
