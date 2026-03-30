using MediatR;
using SplashSphere.Application.Features.AttendanceReports.Queries.ExportAttendanceCsv;
using SplashSphere.Application.Features.AttendanceReports.Queries.GetAttendanceReport;

namespace SplashSphere.API.Endpoints;

public static class AttendanceEndpoints
{
    public static IEndpointRouteBuilder MapAttendanceEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/attendance")
            .WithTags("Attendance")
            .RequireAuthorization();

        group.MapGet("/report", GetReport).WithName("GetAttendanceReport");
        group.MapGet("/export/csv", ExportCsv).WithName("ExportAttendanceCsv");

        return app;
    }

    private static async Task<IResult> GetReport(
        DateOnly from,
        DateOnly to,
        string? branchId,
        string? employeeId,
        int? expectedWorkDaysPerWeek,
        ISender sender,
        CancellationToken ct)
        => TypedResults.Ok(
            await sender.Send(
                new GetAttendanceReportQuery(from, to, branchId, employeeId,
                    expectedWorkDaysPerWeek ?? 6), ct));

    private static async Task<IResult> ExportCsv(
        DateOnly from,
        DateOnly to,
        string? branchId,
        string? employeeId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new ExportAttendanceCsvQuery(from, to, branchId, employeeId), ct);
        return TypedResults.File(result.Content, "text/csv", result.FileName);
    }
}
