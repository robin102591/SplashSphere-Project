using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Employees.Commands.ClockIn;
using SplashSphere.Application.Features.Employees.Commands.ClockOut;
using SplashSphere.Application.Features.Employees.Commands.CreateEmployee;
using SplashSphere.Application.Features.Employees.Commands.ToggleEmployeeStatus;
using SplashSphere.Application.Features.Employees.Commands.UpdateEmployee;
using SplashSphere.Application.Features.Employees.Queries.GetAttendance;
using SplashSphere.Application.Features.Employees.Queries.GetEmployeeById;
using SplashSphere.Application.Features.Employees.Queries.GetEmployeeCommissions;
using SplashSphere.Application.Features.Employees.Queries.GetEmployees;

namespace SplashSphere.API.Endpoints;

public static class EmployeeEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/employees")
            .WithTags("Employees")
            .RequireAuthorization();

        group.MapGet("/",                              GetEmployees)          .WithName("GetEmployees");
        group.MapGet("/{id}",                          GetEmployeeById)       .WithName("GetEmployeeById");
        group.MapPost("/",                             CreateEmployee)        .WithName("CreateEmployee");
        group.MapPut("/{id}",                          UpdateEmployee)        .WithName("UpdateEmployee");
        group.MapPatch("/{id}/status",                 ToggleEmployeeStatus)  .WithName("ToggleEmployeeStatus");
        group.MapPost("/{id}/clock-in",                ClockIn)               .WithName("ClockIn");
        group.MapPost("/{id}/clock-out",               ClockOut)              .WithName("ClockOut");
        group.MapGet("/{id}/commissions",              GetCommissions)        .WithName("GetEmployeeCommissions");
        group.MapGet("/attendance",                    GetAttendance)         .WithName("GetAttendance");

        return app;
    }

    // ── GET /api/v1/employees?branchId=&employeeType=&search=&page=&pageSize= ─

    private static async Task<IResult> GetEmployees(
        [AsParameters] GetEmployeesQuery query, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    // ── GET /api/v1/employees/{id} ────────────────────────────────────────────

    private static async Task<IResult> GetEmployeeById(
        string id, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetEmployeeByIdQuery(id), ct));

    // ── POST /api/v1/employees ────────────────────────────────────────────────

    private static async Task<IResult> CreateEmployee(
        CreateEmployeeCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/employees/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    // ── PUT /api/v1/employees/{id} ────────────────────────────────────────────

    private static async Task<IResult> UpdateEmployee(
        string id, UpdateEmployeeRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateEmployeeCommand(
                id, body.FirstName, body.LastName, body.DailyRate,
                body.Email, body.ContactNumber, body.HiredDate),
            ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PATCH /api/v1/employees/{id}/status ──────────────────────────────────

    private static async Task<IResult> ToggleEmployeeStatus(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ToggleEmployeeStatusCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── POST /api/v1/employees/{id}/clock-in ─────────────────────────────────

    private static async Task<IResult> ClockIn(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ClockInCommand(id), ct);
        return result.IsSuccess
            ? TypedResults.Ok(new { attendanceId = result.Value })
            : result.ToProblem();
    }

    // ── POST /api/v1/employees/{id}/clock-out ────────────────────────────────

    private static async Task<IResult> ClockOut(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ClockOutCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── GET /api/v1/employees/{id}/commissions?from=&to=&page=&pageSize= ─────

    private static async Task<IResult> GetCommissions(
        string id, [AsParameters] CommissionsParams p, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(
            new GetEmployeeCommissionsQuery(id, p.Page, p.PageSize, p.From, p.To), ct));

    // ── GET /api/v1/employees/attendance?branchId=&employeeId=&from=&to= ─────

    private static async Task<IResult> GetAttendance(
        [AsParameters] GetAttendanceQuery query, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    // ── Request / query-string param types ───────────────────────────────────

    private sealed record UpdateEmployeeRequest(
        string FirstName,
        string LastName,
        decimal? DailyRate,
        string? Email,
        string? ContactNumber,
        DateOnly? HiredDate);

    /// <summary>
    /// Separate params record for /commissions so the route-bound {id} stays clean
    /// and doesn't bleed into the query-string binding.
    /// </summary>
    private sealed record CommissionsParams(
        int Page = 1,
        int PageSize = 20,
        DateOnly? From = null,
        DateOnly? To = null);
}
