using MediatR;
using SplashSphere.Application.Features.Connect.History.Queries.GetServiceHistory;

namespace SplashSphere.API.Endpoints.Connect;

/// <summary>
/// Connect app service-history endpoint under <c>/api/v1/connect/history</c>.
/// Aggregates completed transactions across every tenant the customer has joined.
/// Requires a valid Connect JWT.
/// </summary>
public static class ConnectHistoryEndpoints
{
    public static IEndpointRouteBuilder MapConnectHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapConnectGroup("/api/v1/connect/history", "Connect.History");

        // GET /api/v1/connect/history?take=50
        group.MapGet("/", async (int? take, ISender sender, CancellationToken ct) =>
        {
            var items = await sender.Send(new GetServiceHistoryQuery(take), ct);
            return Results.Ok(items);
        })
        .WithName("Connect.GetServiceHistory")
        .WithSummary("Cross-tenant service history for the authenticated customer.");

        return app;
    }
}
