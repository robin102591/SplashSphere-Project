using MediatR;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.Display.Queries.GetDisplayConfig;

namespace SplashSphere.API.Endpoints;

/// <summary>
/// Read-only endpoints consumed by the customer-facing display device. The
/// admin manages the underlying display settings under <c>/api/v1/settings/display</c>;
/// these endpoints exist so the display tablet has a single, purpose-shaped
/// payload (settings + customer-safe branding) without joining two requests.
/// </summary>
public static class DisplayEndpoints
{
    public static IEndpointRouteBuilder MapDisplayEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/display")
            .RequireAuthorization()
            .WithTags("Customer Display");

        group.MapGet("/config", GetDisplayConfig)
            .WithName("GetDisplayConfig")
            .WithSummary("Combined render config for the display device: settings (with branch fallback) + customer-safe tenant branding.");

        return app;
    }

    private static async Task<IResult> GetDisplayConfig(
        [FromQuery] string? branchId,
        ISender sender,
        CancellationToken ct)
    {
        var config = await sender.Send(new GetDisplayConfigQuery(branchId), ct);
        return TypedResults.Ok(config);
    }
}
