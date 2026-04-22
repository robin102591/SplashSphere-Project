using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Booking.Commands.CancelBooking;

/// <summary>
/// Cancel a booking. Only the owner may cancel; only allowed when the booking
/// is in <c>Confirmed</c> or <c>Arrived</c> state (cannot cancel once service
/// has begun or the booking is already terminal).
/// </summary>
public sealed record CancelBookingCommand(string BookingId, string? Reason) : ICommand;
