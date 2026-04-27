using MediatR;
using SplashSphere.Application.Features.Connect.Queue.Queries.GetActiveQueuePosition;

namespace SplashSphere.API.Endpoints.Connect;

/// <summary>
/// Connect app live-queue endpoints under <c>/api/v1/connect/queue</c>.
/// Requires a valid Connect JWT.
/// </summary>
public static class ConnectQueueEndpoints
{
    public static IEndpointRouteBuilder MapConnectQueueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapConnectGroup("/api/v1/connect/queue", "Connect.Queue");

        // GET /api/v1/connect/queue/active
        group.MapGet("/active", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetActiveQueuePositionQuery(), ct);
            return result is null ? Results.NoContent() : Results.Ok(result);
        })
        .WithName("Connect.GetActiveQueuePosition")
        .WithSummary("Get the caller's currently active queue entry (any tenant), if any.");

        return app;
    }
}
