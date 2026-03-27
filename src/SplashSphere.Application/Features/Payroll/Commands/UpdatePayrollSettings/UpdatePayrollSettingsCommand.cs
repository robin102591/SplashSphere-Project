using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollSettings;

public sealed record UpdatePayrollSettingsCommand(
    int CutOffStartDay,        // DayOfWeek as int: 0–6
    int Frequency = 1,         // PayrollFrequency as int: 1=Weekly, 2=SemiMonthly
    int PayReleaseDayOffset = 3,
    bool AutoCalcGovernmentDeductions = false) : ICommand;
