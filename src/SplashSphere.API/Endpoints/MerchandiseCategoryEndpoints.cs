using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.MerchandiseCategories.Commands.CreateMerchandiseCategory;
using SplashSphere.Application.Features.MerchandiseCategories.Commands.ToggleMerchandiseCategoryStatus;
using SplashSphere.Application.Features.MerchandiseCategories.Commands.UpdateMerchandiseCategory;
using SplashSphere.Application.Features.MerchandiseCategories.Queries.GetMerchandiseCategoryById;
using SplashSphere.Application.Features.MerchandiseCategories.Queries.GetMerchandiseCategories;

namespace SplashSphere.API.Endpoints;

public static class MerchandiseCategoryEndpoints
{
    public static IEndpointRouteBuilder MapMerchandiseCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/merchandise-categories")
            .WithTags("MerchandiseCategories")
            .RequireAuthorization();

        group.MapGet("/",     GetMerchandiseCategories)  .WithName("GetMerchandiseCategories");
        group.MapGet("/{id}", GetMerchandiseCategoryById) .WithName("GetMerchandiseCategoryById");
        group.MapPost("/",    CreateMerchandiseCategory)  .WithName("CreateMerchandiseCategory");
        group.MapPut("/{id}", UpdateMerchandiseCategory)  .WithName("UpdateMerchandiseCategory");
        group.MapPatch("/{id}/status", ToggleMerchandiseCategoryStatus).WithName("ToggleMerchandiseCategoryStatus");

        return app;
    }

    private static async Task<IResult> GetMerchandiseCategories(
        [AsParameters] GetMerchandiseCategoriesQuery query, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    private static async Task<IResult> GetMerchandiseCategoryById(
        string id, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetMerchandiseCategoryByIdQuery(id), ct));

    private static async Task<IResult> CreateMerchandiseCategory(
        CreateMerchandiseCategoryCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/merchandise-categories/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> UpdateMerchandiseCategory(
        string id, UpdateMerchandiseCategoryRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateMerchandiseCategoryCommand(id, body.Name, body.Description), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> ToggleMerchandiseCategoryStatus(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ToggleMerchandiseCategoryStatusCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private sealed record UpdateMerchandiseCategoryRequest(string Name, string? Description);
}
