using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Booking.Queries.GetBookingDetail;

/// <summary>
/// Read detail for a single booking — including services and queue/transaction
/// linkage. Returns null when the booking doesn't exist or isn't owned by the
/// authenticated caller.
/// </summary>
public sealed record GetBookingDetailQuery(string BookingId) : IQuery<BookingDetailDto?>;
