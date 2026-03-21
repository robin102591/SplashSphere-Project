using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Shifts.Queries.GetShiftSettings;

public sealed record GetShiftSettingsQuery : IQuery<ShiftSettingsDto>;

public sealed record ShiftSettingsDto(
    decimal DefaultOpeningFund,
    decimal AutoApproveThreshold,
    decimal FlagThreshold,
    bool RequireShiftForTransactions,
    TimeOnly EndOfDayReminderTime);
