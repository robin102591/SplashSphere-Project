using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.BookingSettings.Queries.GetBookingSetting;

/// <summary>
/// Fetch the online-booking configuration for one branch. Returns a DTO populated
/// with defaults when no row exists (lazy-upsert pattern — the row is created on
/// first PUT, not on read).
/// </summary>
public sealed record GetBookingSettingQuery(string BranchId)
    : IQuery<BookingSettingDto>;
