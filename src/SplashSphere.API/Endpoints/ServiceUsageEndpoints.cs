using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.Inventory;
using SplashSphere.Application.Features.Inventory.Commands.UpdateServiceSupplyUsage;
using SplashSphere.Application.Features.Inventory.Queries.GetServiceCostBreakdown;
using SplashSphere.Application.Features.Inventory.Queries.GetServiceSupplyUsage;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class ServiceUsageEndpoints
{
    public static IEndpointRouteBuilder MapServiceUsageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/services/{id}/supply-usage", GetServiceSupplyUsage)
            .RequireAuthorization()
            .WithTags("Inventory")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.SupplyUsageAutoDeduction))
            .WithSummary("Get supply usage configuration for a service");

        app.MapPut("/api/v1/services/{id}/supply-usage", UpdateServiceSupplyUsage)
            .RequireAuthorization()
            .WithTags("Inventory")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.SupplyUsageAutoDeduction))
            .WithSummary("Update supply usage configuration for a service");

        app.MapGet("/api/v1/services/{id}/cost-breakdown", GetServiceCostBreakdown)
            .RequireAuthorization()
            .WithTags("Inventory")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.CostPerWashReports))
            .WithSummary("Get cost breakdown per size for a service");

        return app;
    }

    private static async Task<IResult> GetServiceSupplyUsage(
        string id, ISender sender, CancellationToken ct)
        => TypedResults.Ok(await sender.Send(new GetServiceSupplyUsageQuery(id), ct));

    private static async Task<Results<NoContent, BadRequest<ProblemDetails>>> UpdateServiceSupplyUsage(
        string id, [FromBody] UpdateServiceSupplyUsageRequest body, ISender sender, CancellationToken ct)
    {
        var usages = body.Usages.Select(u => new UsageEntry(u.SupplyItemId, u.SizeId, u.QuantityPerUse)).ToList();
        var result = await sender.Send(new UpdateServiceSupplyUsageCommand(id, usages), ct);

        if (result.IsFailure)
            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetServiceCostBreakdown(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetServiceCostBreakdownQuery(id), ct);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    // Request records
    private sealed record UsageEntryRequest(string SupplyItemId, string? SizeId, decimal QuantityPerUse);

    private sealed record UpdateServiceSupplyUsageRequest(IReadOnlyList<UsageEntryRequest> Usages);
}
