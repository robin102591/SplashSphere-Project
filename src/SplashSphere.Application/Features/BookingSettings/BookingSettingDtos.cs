namespace SplashSphere.Application.Features.BookingSettings;

/// <summary>
/// Admin-facing snapshot of a branch's online-booking configuration.
/// Returned from both the GET (defaults when no row exists) and PUT (upsert) endpoints.
/// </summary>
public sealed record BookingSettingDto(
    string BranchId,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    int SlotIntervalMinutes,
    int MaxBookingsPerSlot,
    int AdvanceBookingDays,
    int MinLeadTimeMinutes,
    int NoShowGraceMinutes,
    bool IsBookingEnabled,
    bool ShowInPublicDirectory);
