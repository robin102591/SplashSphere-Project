using MediatR;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Connect.Discovery.Commands.JoinCarWash;
using SplashSphere.Application.Features.Connect.Discovery.Queries.GetCarWashDetail;
using SplashSphere.Application.Features.Connect.Discovery.Queries.GetMyCarWashes;
using SplashSphere.Application.Features.Connect.Discovery.Queries.SearchCarWashes;

namespace SplashSphere.API.Endpoints.Connect;

/// <summary>
/// Connect app car-wash discovery and join endpoints under
/// <c>/api/v1/connect/carwashes</c> (plus <c>/api/v1/connect/my-carwashes</c>).
/// Requires a valid Connect JWT.
/// </summary>
public static class ConnectDiscoveryEndpoints
{
    public static IEndpointRouteBuilder MapConnectDiscoveryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapConnectGroup("/api/v1/connect", "Connect.Discovery");

        // GET /api/v1/connect/carwashes?search=...&lat=...&lng=...&take=...
        group.MapGet("/carwashes", async (
            string? search,
            decimal? lat,
            decimal? lng,
            int? take,
            ISender sender,
            CancellationToken ct) =>
        {
            var items = await sender.Send(
                new SearchCarWashesQuery(search, lat, lng, take ?? 50), ct);
            return Results.Ok(items);
        })
        .WithName("Connect.SearchCarWashes")
        .WithSummary("Search the public directory of SplashSphere car washes.");

        // GET /api/v1/connect/carwashes/{tenantId}
        group.MapGet("/carwashes/{tenantId}", async (
            string tenantId,
            ISender sender,
            CancellationToken ct) =>
        {
            var detail = await sender.Send(new GetCarWashDetailQuery(tenantId), ct);
            return detail is null ? Results.NotFound() : Results.Ok(detail);
        })
        .WithName("Connect.GetCarWashDetail")
        .WithSummary("Read a car wash's public detail (branches + services).");

        // POST /api/v1/connect/carwashes/{tenantId}/join
        group.MapPost("/carwashes/{tenantId}/join", async (
            string tenantId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new JoinCarWashCommand(tenantId), ct);
            return result.IsFailure ? result.ToProblem() : Results.NoContent();
        })
        .WithName("Connect.JoinCarWash")
        .WithSummary("Link the authenticated user to this car wash.");

        // GET /api/v1/connect/my-carwashes
        group.MapGet("/my-carwashes", async (ISender sender, CancellationToken ct) =>
        {
            var items = await sender.Send(new GetMyCarWashesQuery(), ct);
            return Results.Ok(items);
        })
        .WithName("Connect.GetMyCarWashes")
        .WithSummary("List all car washes the user has joined.");

        return app;
    }
}
