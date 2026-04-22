using MediatR;
using SplashSphere.Application.Features.Connect.Services.Queries.GetServicesWithPricing;

namespace SplashSphere.API.Endpoints.Connect;

/// <summary>
/// Connect app tenant-services pricing endpoint under
/// <c>/api/v1/connect/carwashes/{tenantId}/services</c>.
/// Requires a valid Connect JWT.
/// </summary>
public static class ConnectServicesEndpoints
{
    public static IEndpointRouteBuilder MapConnectServicesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapConnectGroup("/api/v1/connect/carwashes", "Connect.Services");

        // GET /api/v1/connect/carwashes/{tenantId}/services?vehicleId={id}
        group.MapGet("/{tenantId}/services", async (
            string tenantId,
            string vehicleId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new GetServicesWithPricingQuery(tenantId, vehicleId), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("Connect.GetServicesWithPricing")
        .WithSummary("List tenant services with exact or estimated pricing for the selected vehicle.");

        return app;
    }
}
