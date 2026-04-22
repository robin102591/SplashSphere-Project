using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Bookings.Commands.ClassifyBookingVehicle;

/// <summary>
/// Cashier-driven classification of a booking's vehicle at arrival.
/// <para>
/// Creates (or reuses) a tenant <see cref="Domain.Entities.Car"/> matching the
/// <see cref="Domain.Entities.ConnectVehicle"/>'s plate number, pins the
/// booking to it, and locks exact prices on every <see cref="Domain.Entities.BookingService"/>
/// line using the <see cref="Domain.Entities.ServicePricing"/> matrix for the
/// chosen (VehicleTypeId, SizeId).
/// </para>
/// <para>
/// Only bookings in <see cref="Domain.Enums.BookingStatus.Confirmed"/> or
/// <see cref="Domain.Enums.BookingStatus.Arrived"/> can be classified — and
/// only once (<see cref="Domain.Entities.Booking.IsVehicleClassified"/> must be <c>false</c>).
/// </para>
/// </summary>
public sealed record ClassifyBookingVehicleCommand(
    string BookingId,
    string VehicleTypeId,
    string SizeId) : ICommand<BookingClassificationResultDto>;

/// <summary>Result of a successful classification — for the POS "estimate → actual" display.</summary>
public sealed record BookingClassificationResultDto(
    string BookingId,
    string CarId,
    decimal Total,
    IReadOnlyList<ClassifiedBookingServiceDto> Services);

/// <summary>Per-service locked-in price after classification.</summary>
public sealed record ClassifiedBookingServiceDto(
    string ServiceId,
    string ServiceName,
    decimal Price);
