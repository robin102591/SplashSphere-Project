using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.BookingSettings;
using SplashSphere.Application.Features.BookingSettings.Commands.UpsertBookingSetting;
using SplashSphere.Application.Features.BookingSettings.Queries.GetBookingSetting;
using SplashSphere.Domain.Subscription;
using SplashSphere.Infrastructure.Authentication;

namespace SplashSphere.API.Endpoints;

public static class BookingSettingEndpoints
{
    public static IEndpointRouteBuilder MapBookingSettingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/booking-settings")
            .RequireAuthorization()
            .WithTags("BookingSettings")
            .WithMetadata(new RequiresFeatureAttribute(FeatureKeys.OnlineBooking));

        group.MapGet("/", Get)
            .WithSummary("Get booking settings for a branch (defaults returned when no row exists)");

        group.MapPut("/", Upsert)
            .WithSummary("Create or update booking settings for a branch");

        return app;
    }

    private static async Task<Ok<BookingSettingDto>> Get(
        [FromQuery] string branchId,
        ISender sender,
        CancellationToken ct)
    {
        var dto = await sender.Send(new GetBookingSettingQuery(branchId), ct);
        return TypedResults.Ok(dto);
    }

    private static async Task<Results<Ok<BookingSettingDto>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>> Upsert(
        [FromQuery] string branchId,
        [FromBody]  UpsertBookingSettingRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpsertBookingSettingCommand(
            branchId,
            body.OpenTime,
            body.CloseTime,
            body.SlotIntervalMinutes,
            body.MaxBookingsPerSlot,
            body.AdvanceBookingDays,
            body.MinLeadTimeMinutes,
            body.NoShowGraceMinutes,
            body.IsBookingEnabled,
            body.ShowInPublicDirectory);

        var result = await sender.Send(command, ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == "NOT_FOUND")
                return TypedResults.NotFound(new ProblemDetails { Detail = result.Error.Message });

            return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });
        }

        return TypedResults.Ok(result.Value);
    }

    private sealed record UpsertBookingSettingRequest(
        TimeOnly OpenTime,
        TimeOnly CloseTime,
        int SlotIntervalMinutes,
        int MaxBookingsPerSlot,
        int AdvanceBookingDays,
        int MinLeadTimeMinutes,
        int NoShowGraceMinutes,
        bool IsBookingEnabled,
        bool ShowInPublicDirectory);
}
