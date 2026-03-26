using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollSettings;

public sealed record UpdatePayrollSettingsCommand(
    int CutOffStartDay) : ICommand; // DayOfWeek as int: 0–6
