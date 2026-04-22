using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Booking.Commands.MarkArrived;

public sealed class MarkArrivedCommandHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<MarkArrivedCommand, Result>
{
    public async Task<Result> Handle(MarkArrivedCommand request, CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated)
        {
            return Result.Failure(Error.Unauthorized("Sign in required."));
        }

        var userId = connectUser.ConnectUserId;

        var booking = await db.Bookings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                b => b.Id == request.BookingId && b.ConnectUserId == userId,
                cancellationToken);
        if (booking is null)
        {
            return Result.Failure(Error.NotFound("Booking", request.BookingId));
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            return Result.Failure(Error.Validation(
                "BOOKING_NOT_ARRIVABLE",
                $"A booking in '{booking.Status}' state cannot be marked arrived."));
        }

        booking.Status = BookingStatus.Arrived;

        // If the pre-arrival queue entry already exists, keep it Waiting — the
        // cashier transitions it to Called/InService. Nothing to touch here.

        // UoWBehavior saves.
        return Result.Success();
    }
}
