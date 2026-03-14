using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Sizes.Commands.CreateSize;
using SplashSphere.Application.Features.Sizes.Commands.ToggleSizeStatus;
using SplashSphere.Application.Features.Sizes.Commands.UpdateSize;
using SplashSphere.Application.Features.Sizes.Queries.GetSizeById;
using SplashSphere.Application.Features.Sizes.Queries.GetSizes;

namespace SplashSphere.API.Endpoints;

public static class SizeEndpoints
{
    public static IEndpointRouteBuilder MapSizeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/sizes")
            .WithTags("Sizes")
            .RequireAuthorization();

        group.MapGet("/",     GetSizes)  .WithName("GetSizes");
        group.MapGet("/{id}", GetSizeById).WithName("GetSizeById");
        group.MapPost("/",    CreateSize) .WithName("CreateSize");
        group.MapPut("/{id}", UpdateSize) .WithName("UpdateSize");
        group.MapPatch("/{id}/status", ToggleSizeStatus).WithName("ToggleSizeStatus");

        return app;
    }

    private static async Task<IResult> GetSizes(
        [AsParameters] GetSizesQuery query, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    private static async Task<IResult> GetSizeById(
        string id, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetSizeByIdQuery(id), ct));

    private static async Task<IResult> CreateSize(
        CreateSizeCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/sizes/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> UpdateSize(
        string id, UpdateSizeRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateSizeCommand(id, body.Name), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> ToggleSizeStatus(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ToggleSizeStatusCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private sealed record UpdateSizeRequest(string Name);
}
