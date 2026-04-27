using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Bookings.Commands.CheckInBooking;

/// <summary>
/// Cashier-driven booking check-in at the POS. Transitions a booking from
/// <see cref="BookingStatus.Confirmed"/> → <see cref="BookingStatus.Arrived"/>.
/// <para>
/// <b>Idempotent</b>: re-running against an already-Arrived / InService /
/// Completed booking returns the current state as success (so the cashier
/// can double-click without errors). Cancelled or NoShow is a failure.
/// </para>
/// <para>
/// If the pre-slot Hangfire job hasn't yet created a <c>QueueEntry</c> for
/// this booking (customer arrived early), the handler creates one now at
/// <see cref="QueuePriority.Booked"/> priority.
/// </para>
/// </summary>
public sealed record CheckInBookingCommand(string BookingId)
    : ICommand<BookingCheckInDto>;

/// <summary>Result of a successful cashier check-in.</summary>
public sealed record BookingCheckInDto(
    string BookingId,
    string? QueueEntryId,
    string? QueueNumber,
    BookingStatus Status);
