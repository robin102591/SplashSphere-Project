using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.UpdateShiftSettings;

public sealed record UpdateShiftSettingsCommand(
    decimal DefaultOpeningFund,
    decimal AutoApproveThreshold,
    decimal FlagThreshold,
    bool RequireShiftForTransactions,
    TimeOnly EndOfDayReminderTime) : ICommand;
