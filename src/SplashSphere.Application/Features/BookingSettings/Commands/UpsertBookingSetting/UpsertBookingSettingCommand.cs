using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.BookingSettings.Commands.UpsertBookingSetting;

/// <summary>
/// Create or update the booking configuration for a branch.
/// TenantId is sourced from the authenticated tenant context.
/// </summary>
public sealed record UpsertBookingSettingCommand(
    string BranchId,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    int SlotIntervalMinutes,
    int MaxBookingsPerSlot,
    int AdvanceBookingDays,
    int MinLeadTimeMinutes,
    int NoShowGraceMinutes,
    bool IsBookingEnabled,
    bool ShowInPublicDirectory)
    : ICommand<BookingSettingDto>;
