using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Booking.Commands.CancelBooking;

public sealed class CancelBookingCommandHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<CancelBookingCommand, Result>
{
    public async Task<Result> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
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

        if (booking.Status is not (BookingStatus.Confirmed or BookingStatus.Arrived))
        {
            return Result.Failure(Error.Validation(
                "BOOKING_NOT_CANCELLABLE",
                $"A booking in '{booking.Status}' state cannot be cancelled."));
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = string.IsNullOrWhiteSpace(request.Reason)
            ? null
            : request.Reason.Trim();

        // Cascade to queue entry if one was already created.
        if (!string.IsNullOrEmpty(booking.QueueEntryId))
        {
            var queueEntry = await db.QueueEntries
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(q => q.Id == booking.QueueEntryId, cancellationToken);
            if (queueEntry is not null
                && queueEntry.Status is QueueStatus.Waiting or QueueStatus.Called)
            {
                queueEntry.Status = QueueStatus.Cancelled;
                queueEntry.CancelledAt = DateTime.UtcNow;
            }
        }

        // UoWBehavior saves.
        return Result.Success();
    }
}
