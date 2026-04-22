using MediatR;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Connect.Booking.Commands.CancelBooking;
using SplashSphere.Application.Features.Connect.Booking.Commands.CreateBooking;
using SplashSphere.Application.Features.Connect.Booking.Commands.MarkArrived;
using SplashSphere.Application.Features.Connect.Booking.Queries.GetAvailableSlots;
using SplashSphere.Application.Features.Connect.Booking.Queries.GetBookingDetail;
using SplashSphere.Application.Features.Connect.Booking.Queries.GetMyBookings;

namespace SplashSphere.API.Endpoints.Connect;

/// <summary>
/// Connect app booking endpoints (slots, CRUD, arrive).
/// Requires a valid Connect JWT.
/// </summary>
public static class ConnectBookingEndpoints
{
    public static IEndpointRouteBuilder MapConnectBookingEndpoints(this IEndpointRouteBuilder app)
    {
        var carwashes = app.MapConnectGroup("/api/v1/connect/carwashes", "Connect.Booking");
        var bookings = app.MapConnectGroup("/api/v1/connect/bookings", "Connect.Booking");

        // GET /api/v1/connect/carwashes/{tenantId}/slots?branchId=...&date=YYYY-MM-DD
        carwashes.MapGet("/{tenantId}/slots", async (
            string tenantId,
            string branchId,
            DateOnly date,
            ISender sender,
            CancellationToken ct) =>
        {
            var slots = await sender.Send(
                new GetAvailableSlotsQuery(tenantId, branchId, date), ct);
            return Results.Ok(slots);
        })
        .WithName("Connect.GetAvailableSlots")
        .WithSummary("List available booking slots for a branch on a date.");

        // POST /api/v1/connect/bookings
        bookings.MapPost("/", async (
            [FromBody] CreateBookingRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateBookingCommand(
                body.TenantId,
                body.BranchId,
                body.VehicleId,
                body.SlotStartUtc,
                body.ServiceIds), ct);
            return result.IsFailure
                ? result.ToProblem()
                : Results.Created($"/api/v1/connect/bookings/{result.Value!.Id}", result.Value);
        })
        .WithName("Connect.CreateBooking")
        .WithSummary("Create a booking at a car wash for the authenticated user.");

        // GET /api/v1/connect/bookings?includePast=true
        bookings.MapGet("/", async (
            bool? includePast,
            ISender sender,
            CancellationToken ct) =>
        {
            var items = await sender.Send(
                new GetMyBookingsQuery(includePast ?? false), ct);
            return Results.Ok(items);
        })
        .WithName("Connect.GetMyBookings")
        .WithSummary("List the authenticated user's bookings.");

        // GET /api/v1/connect/bookings/{id}
        bookings.MapGet("/{id}", async (
            string id,
            ISender sender,
            CancellationToken ct) =>
        {
            var detail = await sender.Send(new GetBookingDetailQuery(id), ct);
            return detail is null ? Results.NotFound() : Results.Ok(detail);
        })
        .WithName("Connect.GetBookingDetail")
        .WithSummary("Read a booking's detail + queue status.");

        // PATCH /api/v1/connect/bookings/{id}/cancel
        bookings.MapPatch("/{id}/cancel", async (
            string id,
            [FromBody] CancelBookingRequest? body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CancelBookingCommand(id, body?.Reason), ct);
            return result.IsFailure ? result.ToProblem() : Results.NoContent();
        })
        .WithName("Connect.CancelBooking")
        .WithSummary("Cancel a booking.");

        // PATCH /api/v1/connect/bookings/{id}/arrived
        bookings.MapPatch("/{id}/arrived", async (
            string id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new MarkArrivedCommand(id), ct);
            return result.IsFailure ? result.ToProblem() : Results.NoContent();
        })
        .WithName("Connect.MarkBookingArrived")
        .WithSummary("Self check-in — mark the booking as arrived.");

        return app;
    }
}

// ── Request bodies ──────────────────────────────────────────────────────────

internal sealed record CreateBookingRequest(
    string TenantId,
    string BranchId,
    string VehicleId,
    DateTime SlotStartUtc,
    IReadOnlyList<string> ServiceIds);

internal sealed record CancelBookingRequest(string? Reason);
