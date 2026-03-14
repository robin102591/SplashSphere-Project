using MediatR;
using SplashSphere.Application.Features.Reports.Queries.GetCommissionsReport;
using SplashSphere.Application.Features.Reports.Queries.GetRevenueReport;
using SplashSphere.Application.Features.Reports.Queries.GetServicePopularityReport;

namespace SplashSphere.API.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/reports")
            .WithTags("Reports")
            .RequireAuthorization();

        // GET /api/v1/reports/revenue?from=2025-01-01&to=2025-01-31&branchId=
        group.MapGet("/revenue", GetRevenue).WithName("GetRevenueReport");

        // GET /api/v1/reports/commissions?from=2025-01-01&to=2025-01-31&branchId=&employeeId=
        group.MapGet("/commissions", GetCommissions).WithName("GetCommissionsReport");

        // GET /api/v1/reports/service-popularity?from=2025-01-01&to=2025-01-31&branchId=&top=20
        group.MapGet("/service-popularity", GetServicePopularity).WithName("GetServicePopularityReport");

        return app;
    }

    // ── Revenue ───────────────────────────────────────────────────────────────

    private static async Task<IResult> GetRevenue(
        DateOnly from,
        DateOnly to,
        string? branchId,
        ISender sender,
        CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(new GetRevenueReportQuery(from, to, branchId), ct));

    // ── Commissions ───────────────────────────────────────────────────────────

    private static async Task<IResult> GetCommissions(
        DateOnly from,
        DateOnly to,
        string? branchId,
        string? employeeId,
        ISender sender,
        CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(new GetCommissionsReportQuery(from, to, branchId, employeeId), ct));

    // ── Service popularity ────────────────────────────────────────────────────

    private static async Task<IResult> GetServicePopularity(
        DateOnly from,
        DateOnly to,
        string? branchId,
        int top,
        ISender sender,
        CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(new GetServicePopularityReportQuery(from, to, branchId, top == 0 ? 20 : top), ct));
}
