using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Booking.Queries.GetAvailableSlots;

/// <summary>
/// Get available booking slots for a given branch on a specific Manila-local date.
/// Respects the branch's <c>BookingSetting</c> for operating hours, slot interval,
/// per-slot capacity, advance window, and lead time. Returns an empty list when
/// booking is disabled or the date is outside the permitted window.
/// </summary>
public sealed record GetAvailableSlotsQuery(
    string TenantId,
    string BranchId,
    DateOnly Date) : IQuery<IReadOnlyList<BookingSlotDto>>;
