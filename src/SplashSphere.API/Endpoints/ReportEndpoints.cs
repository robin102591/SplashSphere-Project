using MediatR;
using SplashSphere.Application.Features.Reports.Queries.ExportCommissionsCsv;
using SplashSphere.Application.Features.Reports.Queries.ExportRevenueCsv;
using SplashSphere.Application.Features.Reports.Queries.ExportServicePopularityCsv;
using SplashSphere.Application.Features.Reports.Queries.GetCommissionsReport;
using SplashSphere.Application.Features.Reports.Queries.GetCustomerAnalytics;
using SplashSphere.Application.Features.Reports.Queries.GetEmployeePerformance;
using SplashSphere.Application.Features.Reports.Queries.GetPeakHours;
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

        group.MapGet("/revenue", GetRevenue).WithName("GetRevenueReport");
        group.MapGet("/commissions", GetCommissions).WithName("GetCommissionsReport");
        group.MapGet("/service-popularity", GetServicePopularity).WithName("GetServicePopularityReport");

        // Analytics
        group.MapGet("/customer-analytics", GetCustomerAnalytics).WithName("GetCustomerAnalytics");
        group.MapGet("/peak-hours", GetPeakHours).WithName("GetPeakHours");
        group.MapGet("/employee-performance", GetEmployeePerformance).WithName("GetEmployeePerformance");

        // CSV exports
        group.MapGet("/revenue/export/csv", ExportRevenueCsv).WithName("ExportRevenueCsv");
        group.MapGet("/commissions/export/csv", ExportCommissionsCsv).WithName("ExportCommissionsCsv");
        group.MapGet("/service-popularity/export/csv", ExportServicePopularityCsv).WithName("ExportServicePopularityCsv");

        return app;
    }

    // ── JSON endpoints ──────────────────────────────────────────────────────

    private static async Task<IResult> GetRevenue(
        DateOnly from, DateOnly to, string? branchId,
        ISender sender, CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(new GetRevenueReportQuery(from, to, branchId), ct));

    private static async Task<IResult> GetCommissions(
        DateOnly from, DateOnly to, string? branchId, string? employeeId,
        ISender sender, CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(new GetCommissionsReportQuery(from, to, branchId, employeeId), ct));

    private static async Task<IResult> GetServicePopularity(
        DateOnly from, DateOnly to, string? branchId, int top,
        ISender sender, CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(new GetServicePopularityReportQuery(from, to, branchId, top == 0 ? 20 : top), ct));

    // ── Analytics endpoints ──────────────────────────────────────────────────

    private static async Task<IResult> GetCustomerAnalytics(
        DateOnly from, DateOnly to, string? branchId,
        ISender sender, CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(new GetCustomerAnalyticsQuery(from, to, branchId), ct));

    private static async Task<IResult> GetPeakHours(
        DateOnly from, DateOnly to, string? branchId,
        ISender sender, CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(new GetPeakHoursQuery(from, to, branchId), ct));

    private static async Task<IResult> GetEmployeePerformance(
        DateOnly from, DateOnly to, string? branchId,
        ISender sender, CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(new GetEmployeePerformanceQuery(from, to, branchId), ct));

    // ── CSV exports ─────────────────────────────────────────────────────────

    private static async Task<IResult> ExportRevenueCsv(
        DateOnly from, DateOnly to, string? branchId,
        ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ExportRevenueCsvQuery(from, to, branchId), ct);
        return TypedResults.File(result.Content, "text/csv", result.FileName);
    }

    private static async Task<IResult> ExportCommissionsCsv(
        DateOnly from, DateOnly to, string? branchId, string? employeeId,
        ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ExportCommissionsCsvQuery(from, to, branchId, employeeId), ct);
        return TypedResults.File(result.Content, "text/csv", result.FileName);
    }

    private static async Task<IResult> ExportServicePopularityCsv(
        DateOnly from, DateOnly to, string? branchId, int top,
        ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ExportServicePopularityCsvQuery(from, to, branchId, top == 0 ? 20 : top), ct);
        return TypedResults.File(result.Content, "text/csv", result.FileName);
    }
}
