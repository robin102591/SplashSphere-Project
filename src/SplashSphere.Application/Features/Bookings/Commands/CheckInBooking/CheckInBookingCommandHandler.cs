using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Bookings.Commands.CheckInBooking;

public sealed class CheckInBookingCommandHandler(
    IApplicationDbContext db,
    IEventPublisher eventPublisher)
    : IRequestHandler<CheckInBookingCommand, Result<BookingCheckInDto>>
{
    private const int MaxQueueNumberRetries = 5;
    private const int DefaultServiceDurationMinutes = 15;
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<Result<BookingCheckInDto>> Handle(
        CheckInBookingCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await db.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking is null)
            return Result.Failure<BookingCheckInDto>(Error.NotFound("Booking", request.BookingId));

        // ── Terminal / non-checkinable states ────────────────────────────────
        if (booking.Status is BookingStatus.Cancelled or BookingStatus.NoShow)
        {
            return Result.Failure<BookingCheckInDto>(Error.Validation(
                "BOOKING_NOT_CHECKINABLE",
                $"A booking in '{booking.Status}' state cannot be checked in."));
        }

        // ── Idempotent path: already past Confirmed → return current state ──
        if (booking.Status is BookingStatus.Arrived
                           or BookingStatus.InService
                           or BookingStatus.Completed)
        {
            string? idempotentQueueNumber = null;
            if (booking.QueueEntryId is not null)
            {
                idempotentQueueNumber = await db.QueueEntries
                    .AsNoTracking()
                    .Where(q => q.Id == booking.QueueEntryId)
                    .Select(q => q.QueueNumber)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return Result.Success(new BookingCheckInDto(
                booking.Id,
                booking.QueueEntryId,
                idempotentQueueNumber,
                booking.Status));
        }

        // booking.Status == Confirmed from here on.
        var now = DateTime.UtcNow;

        string? queueEntryId = booking.QueueEntryId;
        string? queueNumber = null;

        if (queueEntryId is null)
        {
            // Customer arrived before the 15-min Hangfire window — create the
            // queue entry right now at Booked priority.
            var plate = await db.ConnectVehicles
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(v => v.Id == booking.ConnectVehicleId)
                .Select(v => v.PlateNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(plate))
                return Result.Failure<BookingCheckInDto>(Error.NotFound(
                    "ConnectVehicle", booking.ConnectVehicleId));

            var localToday = DateOnly.FromDateTime(DateTime.UtcNow + ManilaOffset);

            var waitingAhead = await db.QueueEntries
                .CountAsync(q => q.BranchId == booking.BranchId
                              && q.Status == QueueStatus.Waiting, cancellationToken);

            var estimatedWait = waitingAhead > 0
                ? waitingAhead * DefaultServiceDurationMinutes
                : (int?)null;

            // Same retry-on-23505 pattern as BookingJobService.
            QueueEntry? createdEntry = null;

            for (var attempt = 0; attempt < MaxQueueNumberRetries; attempt++)
            {
                var todayCount = await db.QueueEntries
                    .CountAsync(q => q.BranchId == booking.BranchId
                                  && q.QueueDate == localToday, cancellationToken);

                var candidateNumber = $"Q-{todayCount + 1:D3}";

                var entry = new QueueEntry(
                    tenantId: booking.TenantId,
                    branchId: booking.BranchId,
                    queueNumber: candidateNumber,
                    queueDate: localToday,
                    plateNumber: plate!,
                    priority: QueuePriority.Booked,
                    customerId: booking.CustomerId,
                    carId: booking.CarId,
                    estimatedWaitMinutes: estimatedWait,
                    preferredServices: null,
                    notes: $"Booking {booking.Id[..8]}");

                db.QueueEntries.Add(entry);
                booking.QueueEntryId = entry.Id;
                booking.Status = BookingStatus.Arrived;

                try
                {
                    await db.SaveChangesAsync(cancellationToken);
                    createdEntry = entry;
                    break;
                }
                catch (DbUpdateException ex)
                    when (ex.InnerException?.Message.Contains("23505") == true ||
                          ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    // Another cashier or the Hangfire job grabbed this sequence
                    // number. Undo and retry.
                    db.QueueEntries.Remove(entry);
                    booking.QueueEntryId = null;
                    booking.Status = BookingStatus.Confirmed;

                    if (attempt == MaxQueueNumberRetries - 1)
                    {
                        return Result.Failure<BookingCheckInDto>(Error.Conflict(
                            "Could not allocate a queue number. Please retry."));
                    }
                }
            }

            if (createdEntry is null)
            {
                return Result.Failure<BookingCheckInDto>(Error.Conflict(
                    "Could not allocate a queue number. Please retry."));
            }

            queueEntryId = createdEntry.Id;
            queueNumber = createdEntry.QueueNumber;

            eventPublisher.Enqueue(new QueueEntryCreatedEvent(
                createdEntry.Id,
                createdEntry.TenantId,
                createdEntry.BranchId,
                createdEntry.QueueNumber,
                createdEntry.PlateNumber,
                QueuePriority.Booked,
                estimatedWait));
        }
        else
        {
            // Queue entry already exists — just flip booking to Arrived.
            booking.Status = BookingStatus.Arrived;

            queueNumber = await db.QueueEntries
                .AsNoTracking()
                .Where(q => q.Id == queueEntryId)
                .Select(q => q.QueueNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Resolve plate for the event payload (may have been loaded above in
        // the early-arrival branch, but we need it either way).
        var platForEvent = await db.ConnectVehicles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(v => v.Id == booking.ConnectVehicleId)
            .Select(v => v.PlateNumber)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        eventPublisher.Enqueue(new BookingArrivedEvent(
            booking.Id,
            booking.TenantId,
            booking.BranchId,
            booking.CustomerId,
            platForEvent,
            booking.SlotStart,
            queueEntryId,
            queueNumber));

        // UoWBehavior saves (or we've already saved inside the retry loop —
        // both paths are safe because the second save is a no-op if there are
        // no pending changes).
        return Result.Success(new BookingCheckInDto(
            booking.Id,
            queueEntryId,
            queueNumber,
            booking.Status));
    }
}
