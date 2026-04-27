using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.BookingAdmin.Queries.GetBookings;

/// <summary>
/// List bookings for the current tenant within a date window.
/// Filters are tenant-scoped (query filters apply). Optional branch and status filters.
/// </summary>
public sealed record GetBookingsQuery(
    DateTime FromDate,
    DateTime ToDate,
    string? BranchId,
    BookingStatus? Status)
    : IQuery<IReadOnlyList<BookingListItemDto>>;
