using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Booking.Queries.GetMyBookings;

/// <summary>
/// List the authenticated customer's bookings. <c>IncludePast</c> defaults to
/// false — only upcoming (SlotStart &gt;= now, non-terminal statuses) are returned.
/// When true, the full history is returned, most recent first.
/// </summary>
public sealed record GetMyBookingsQuery(bool IncludePast = false)
    : IQuery<IReadOnlyList<BookingListItemDto>>;
