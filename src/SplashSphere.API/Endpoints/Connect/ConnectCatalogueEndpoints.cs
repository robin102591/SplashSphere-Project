using MediatR;
using SplashSphere.Application.Features.Connect.Catalogue.Queries.GetGlobalMakes;
using SplashSphere.Application.Features.Connect.Catalogue.Queries.GetGlobalModels;

namespace SplashSphere.API.Endpoints.Connect;

/// <summary>
/// Global vehicle catalogue (shared make/model picker) under
/// <c>/api/v1/connect/catalogue</c>. Requires a valid Connect JWT so only
/// authenticated customers can read the list.
/// </summary>
public static class ConnectCatalogueEndpoints
{
    public static IEndpointRouteBuilder MapConnectCatalogueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapConnectGroup("/api/v1/connect/catalogue", "Connect.Catalogue");

        // GET /api/v1/connect/catalogue/makes
        group.MapGet("/makes", async (ISender sender, CancellationToken ct) =>
        {
            var makes = await sender.Send(new GetGlobalMakesQuery(), ct);
            return Results.Ok(makes);
        })
        .WithName("Connect.GetGlobalMakes")
        .WithSummary("List all active global vehicle makes.");

        // GET /api/v1/connect/catalogue/makes/{makeId}/models
        group.MapGet("/makes/{makeId}/models", async (
            string makeId,
            ISender sender,
            CancellationToken ct) =>
        {
            var models = await sender.Send(new GetGlobalModelsQuery(makeId), ct);
            return Results.Ok(models);
        })
        .WithName("Connect.GetGlobalModels")
        .WithSummary("List active models under a given make.");

        return app;
    }
}
