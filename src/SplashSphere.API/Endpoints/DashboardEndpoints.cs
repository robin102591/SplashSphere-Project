using MediatR;
using SplashSphere.Application.Features.Dashboard.Queries.GetDashboardSummary;

namespace SplashSphere.API.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/dashboard")
            .WithTags("Dashboard")
            .RequireAuthorization();

        // GET /api/v1/dashboard/summary?branchId=
        // branchId is optional; omitting it returns tenant-wide KPIs + branch breakdowns.
        group.MapGet("/summary", GetSummary).WithName("GetDashboardSummary");

        return app;
    }

    private static async Task<IResult> GetSummary(
        string? branchId,
        ISender sender,
        CancellationToken ct)
        => TypedResults.Ok(await sender.Send(new GetDashboardSummaryQuery(branchId), ct));
}
