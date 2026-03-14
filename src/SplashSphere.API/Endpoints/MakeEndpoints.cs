using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Makes.Commands.CreateMake;
using SplashSphere.Application.Features.Makes.Commands.ToggleMakeStatus;
using SplashSphere.Application.Features.Makes.Commands.UpdateMake;
using SplashSphere.Application.Features.Makes.Queries.GetMakeById;
using SplashSphere.Application.Features.Makes.Queries.GetMakes;

namespace SplashSphere.API.Endpoints;

public static class MakeEndpoints
{
    public static IEndpointRouteBuilder MapMakeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/makes")
            .WithTags("Makes")
            .RequireAuthorization();

        group.MapGet("/",     GetMakes)  .WithName("GetMakes");
        group.MapGet("/{id}", GetMakeById).WithName("GetMakeById");
        group.MapPost("/",    CreateMake) .WithName("CreateMake");
        group.MapPut("/{id}", UpdateMake) .WithName("UpdateMake");
        group.MapPatch("/{id}/status", ToggleMakeStatus).WithName("ToggleMakeStatus");

        return app;
    }

    private static async Task<IResult> GetMakes(
        [AsParameters] GetMakesQuery query, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    private static async Task<IResult> GetMakeById(
        string id, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetMakeByIdQuery(id), ct));

    private static async Task<IResult> CreateMake(
        CreateMakeCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/makes/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> UpdateMake(
        string id, UpdateMakeRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateMakeCommand(id, body.Name), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> ToggleMakeStatus(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ToggleMakeStatusCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private sealed record UpdateMakeRequest(string Name);
}
