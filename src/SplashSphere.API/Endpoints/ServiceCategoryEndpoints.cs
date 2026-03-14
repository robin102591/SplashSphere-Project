using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.ServiceCategories.Commands.CreateServiceCategory;
using SplashSphere.Application.Features.ServiceCategories.Commands.ToggleServiceCategoryStatus;
using SplashSphere.Application.Features.ServiceCategories.Commands.UpdateServiceCategory;
using SplashSphere.Application.Features.ServiceCategories.Queries.GetServiceCategoryById;
using SplashSphere.Application.Features.ServiceCategories.Queries.GetServiceCategories;

namespace SplashSphere.API.Endpoints;

public static class ServiceCategoryEndpoints
{
    public static IEndpointRouteBuilder MapServiceCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/service-categories")
            .WithTags("ServiceCategories")
            .RequireAuthorization();

        group.MapGet("/",     GetServiceCategories)  .WithName("GetServiceCategories");
        group.MapGet("/{id}", GetServiceCategoryById) .WithName("GetServiceCategoryById");
        group.MapPost("/",    CreateServiceCategory)  .WithName("CreateServiceCategory");
        group.MapPut("/{id}", UpdateServiceCategory)  .WithName("UpdateServiceCategory");
        group.MapPatch("/{id}/status", ToggleServiceCategoryStatus).WithName("ToggleServiceCategoryStatus");

        return app;
    }

    private static async Task<IResult> GetServiceCategories(
        [AsParameters] GetServiceCategoriesQuery query, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    private static async Task<IResult> GetServiceCategoryById(
        string id, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetServiceCategoryByIdQuery(id), ct));

    private static async Task<IResult> CreateServiceCategory(
        CreateServiceCategoryCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/service-categories/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    private static async Task<IResult> UpdateServiceCategory(
        string id, UpdateServiceCategoryRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateServiceCategoryCommand(id, body.Name, body.Description), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<IResult> ToggleServiceCategoryStatus(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ToggleServiceCategoryStatusCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private sealed record UpdateServiceCategoryRequest(string Name, string? Description);
}
