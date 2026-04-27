using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.BookingAdmin;
using SplashSphere.Application.Features.BookingAdmin.Queries.GetBookingDetailAdmin;
using SplashSphere.Application.Features.BookingAdmin.Queries.GetBookings;
using SplashSphere.Application.Features.Bookings.Commands.CheckInBooking;
using SplashSphere.Application.Features.Bookings.Commands.ClassifyBookingVehicle;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.API.Endpoints;

public static class BookingAdminEndpoints
{
    public static IEndpointRouteBuilder MapBookingAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/bookings")
            .RequireAuthorization()
            .WithTags("Bookings")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.OnlineBooking));

        group.MapGet("/", List)
            .WithSummary("List bookings in a date window for the current tenant");

        group.MapGet("/{id}", GetDetail)
            .WithSummary("Get full booking detail with customer, vehicle, services, and queue/transaction links");

        group.MapPatch("/{id}/check-in", CheckIn)
            .WithSummary("Cashier check-in: flip Confirmed booking to Arrived and allocate a queue entry if needed");

        group.MapPost("/{id}/classify-vehicle", ClassifyVehicle)
            .WithSummary("Classify a booking's vehicle (VehicleType + Size) and lock exact service prices");

        return app;
    }

    private static async Task<Ok<IReadOnlyList<BookingListItemDto>>> List(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? branchId,
        [FromQuery] BookingStatus? status,
        ISender sender,
        CancellationToken ct)
    {
        var rows = await sender.Send(
            new GetBookingsQuery(fromDate, toDate, branchId, status), ct);
        return TypedResults.Ok(rows);
    }

    private static async Task<Results<Ok<BookingAdminDetailDto>, NotFound>> GetDetail(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var dto = await sender.Send(new GetBookingDetailAdminQuery(id), ct);
        return dto is null ? TypedResults.NotFound() : TypedResults.Ok(dto);
    }

    private static async Task<Results<Ok<BookingCheckInDto>, ProblemHttpResult>> CheckIn(
        string id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new CheckInBookingCommand(id), ct);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value!)
            : TypedResults.Problem(
                title: result.Error.Code,
                detail: result.Error.Message,
                statusCode: MapErrorToStatus(result.Error));
    }

    private static async Task<Results<Ok<BookingClassificationResultDto>, ProblemHttpResult>> ClassifyVehicle(
        string id,
        [FromBody] ClassifyVehicleRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new ClassifyBookingVehicleCommand(id, body.VehicleTypeId, body.SizeId), ct);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value!)
            : TypedResults.Problem(
                title: result.Error.Code,
                detail: result.Error.Message,
                statusCode: MapErrorToStatus(result.Error));
    }

    private static int MapErrorToStatus(Error error) => error.Code switch
    {
        "NOT_FOUND"     => StatusCodes.Status404NotFound,
        "CONFLICT"      => StatusCodes.Status409Conflict,
        "UNAUTHORIZED"  => StatusCodes.Status401Unauthorized,
        "FORBIDDEN"     => StatusCodes.Status403Forbidden,
        _               => StatusCodes.Status400BadRequest,
    };

    /// <summary>Request body for the classify-vehicle endpoint.</summary>
    public sealed record ClassifyVehicleRequest(string VehicleTypeId, string SizeId);
}
