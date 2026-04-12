using MediatR;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Customers.Commands.CreateCustomer;
using SplashSphere.Application.Features.Customers.Commands.ToggleCustomerStatus;
using SplashSphere.Application.Features.Customers.Commands.UpdateCustomer;
using SplashSphere.Application.Features.Customers.Queries.GetCustomerById;
using SplashSphere.Application.Features.Customers.Queries.GetCustomers;

namespace SplashSphere.API.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/customers")
            .WithTags("Customers")
            .RequireAuthorization();

        group.MapGet("/",              GetCustomers)         .WithName("GetCustomers").WithSummary("List customers");
        group.MapGet("/{id}",          GetCustomerById)      .WithName("GetCustomerById").WithSummary("Get customer with vehicles and history");
        group.MapPost("/",             CreateCustomer)       .WithName("CreateCustomer").WithSummary("Create customer");
        group.MapPut("/{id}",          UpdateCustomer)       .WithName("UpdateCustomer").WithSummary("Update customer");
        group.MapPatch("/{id}/status", ToggleCustomerStatus) .WithName("ToggleCustomerStatus").WithSummary("Toggle customer active status");

        return app;
    }

    // ── GET /api/v1/customers?search=&page=&pageSize= ─────────────────────────

    private static async Task<IResult> GetCustomers(
        [AsParameters] GetCustomersQuery query, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(query, ct));

    // ── GET /api/v1/customers/{id}  (includes registered cars) ───────────────

    private static async Task<IResult> GetCustomerById(
        string id, ISender sender, CancellationToken ct) =>
        TypedResults.Ok(await sender.Send(new GetCustomerByIdQuery(id), ct));

    // ── POST /api/v1/customers ────────────────────────────────────────────────

    private static async Task<IResult> CreateCustomer(
        CreateCustomerCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/v1/customers/{result.Value}", new { id = result.Value })
            : result.ToProblem();
    }

    // ── PUT /api/v1/customers/{id} ────────────────────────────────────────────

    private static async Task<IResult> UpdateCustomer(
        string id, UpdateCustomerRequest body, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateCustomerCommand(id, body.FirstName, body.LastName, body.Email, body.ContactNumber, body.Notes),
            ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── PATCH /api/v1/customers/{id}/status ──────────────────────────────────

    private static async Task<IResult> ToggleCustomerStatus(
        string id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ToggleCustomerStatusCommand(id), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── Request body types ────────────────────────────────────────────────────

    private sealed record UpdateCustomerRequest(
        string FirstName,
        string LastName,
        string? Email,
        string? ContactNumber,
        string? Notes);
}
