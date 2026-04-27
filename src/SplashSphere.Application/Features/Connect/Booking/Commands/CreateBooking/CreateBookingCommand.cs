using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Booking.Commands.CreateBooking;

/// <summary>
/// Create a booking at <paramref name="TenantId"/>/<paramref name="BranchId"/> for
/// the authenticated Connect user's <paramref name="ConnectVehicleId"/>.
/// <para>
/// <paramref name="SlotStartUtc"/> must exactly match one of the available slots
/// returned by <c>GetAvailableSlotsQuery</c>. The slot end is computed from the
/// branch's configured interval.
/// </para>
/// </summary>
public sealed record CreateBookingCommand(
    string TenantId,
    string BranchId,
    string ConnectVehicleId,
    DateTime SlotStartUtc,
    IReadOnlyList<string> ServiceIds) : ICommand<BookingDetailDto>;
