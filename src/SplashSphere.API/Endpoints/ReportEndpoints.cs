using MediatR;
using SplashSphere.Application.Features.Inventory;
using SplashSphere.Application.Features.Inventory.Queries.GetEquipmentMaintenanceReport;
using SplashSphere.Application.Features.Inventory.Queries.GetInventorySummary;
using SplashSphere.Application.Features.Inventory.Queries.GetPurchaseHistory;
using SplashSphere.Application.Features.Inventory.Queries.GetSupplyUsageTrend;
using SplashSphere.Application.Features.Reports.Queries.ExportCommissionsCsv;
using SplashSphere.Application.Features.Reports.Queries.ExportRevenueCsv;
using SplashSphere.Application.Features.Reports.Queries.ExportServicePopularityCsv;
using SplashSphere.Application.Features.Reports.Queries.GetCommissionsReport;
using SplashSphere.Application.Features.Reports.Queries.GetCustomerAnalytics;
using SplashSphere.Application.Features.Reports.Queries.GetEmployeePerformance;
using SplashSphere.Application.Features.Reports.Queries.GetPeakHours;
using SplashSphere.Application.Features.Reports.Queries.GetRevenueReport;
using SplashSphere.Application.Features.Reports.Queries.GetServicePopularityReport;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

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

        group.MapGet("/revenue", GetRevenue).WithName("GetRevenueReport").WithSummary("Get revenue report by date range and branch");
        group.MapGet("/commissions", GetCommissions).WithName("GetCommissionsReport").WithSummary("Get commissions report by date range, branch, and employee");
        group.MapGet("/service-popularity", GetServicePopularity).WithName("GetServicePopularityReport").WithSummary("Get service popularity rankings by date range");

        // Analytics
        group.MapGet("/customer-analytics", GetCustomerAnalytics).WithName("GetCustomerAnalytics").WithSummary("Get customer analytics including retention and visit frequency");
        group.MapGet("/peak-hours", GetPeakHours).WithName("GetPeakHours").WithSummary("Get peak hours heatmap data by day-of-week and hour");
        group.MapGet("/employee-performance", GetEmployeePerformance).WithName("GetEmployeePerformance").WithSummary("Get employee performance rankings and metrics");

        // Inventory reports
        group.MapGet("/inventory-summary", GetInventorySummary)
            .WithName("GetInventorySummary")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.SupplyTracking))
            .WithSummary("Inventory stock summary with low stock alerts");

        group.MapGet("/supply-usage", GetSupplyUsageTrend)
            .WithName("GetSupplyUsageTrend")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.CostPerWashReports))
            .WithSummary("Supply usage trend over time");

        group.MapGet("/equipment-maintenance", GetEquipmentMaintenanceReport)
            .WithName("GetEquipmentMaintenanceReport")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.EquipmentManagement))
            .WithSummary("Equipment maintenance status report");

        group.MapGet("/purchase-history", GetPurchaseHistory)
            .WithName("GetPurchaseHistory")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.PurchaseOrders))
            .WithSummary("Purchase spending by supplier and category");

        // CSV exports
        group.MapGet("/revenue/export/csv", ExportRevenueCsv).WithName("ExportRevenueCsv").WithSummary("Export revenue report as CSV");
        group.MapGet("/commissions/export/csv", ExportCommissionsCsv).WithName("ExportCommissionsCsv").WithSummary("Export commissions report as CSV");
        group.MapGet("/service-popularity/export/csv", ExportServicePopularityCsv).WithName("ExportServicePopularityCsv").WithSummary("Export service popularity report as CSV");

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

    // ── Inventory report endpoints ──────────────────────────────────────────

    private static async Task<IResult> GetInventorySummary(
        string? branchId,
        ISender sender, CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(new GetInventorySummaryQuery(branchId), ct));

    private static async Task<IResult> GetSupplyUsageTrend(
        DateOnly from, DateOnly to, string? branchId,
        ISender sender, CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(new GetSupplyUsageTrendQuery(from, to, branchId), ct));

    private static async Task<IResult> GetEquipmentMaintenanceReport(
        string? branchId,
        ISender sender, CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(new GetEquipmentMaintenanceReportQuery(branchId), ct));

    private static async Task<IResult> GetPurchaseHistory(
        DateOnly from, DateOnly to, string? branchId,
        ISender sender, CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(new GetPurchaseHistoryQuery(from, to, branchId), ct));

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
