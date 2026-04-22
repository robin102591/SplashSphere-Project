using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Booking.Commands.MarkArrived;

/// <summary>
/// Customer-driven check-in ("I'm here") for a booking. Transitions Confirmed
/// → Arrived. Only the owner may mark arrived; only allowed while the booking
/// is still in <c>Confirmed</c> state.
/// </summary>
public sealed record MarkArrivedCommand(string BookingId) : ICommand;
