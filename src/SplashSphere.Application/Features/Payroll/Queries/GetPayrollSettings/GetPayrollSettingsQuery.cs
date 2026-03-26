using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Queries.GetPayrollSettings;

public sealed record GetPayrollSettingsQuery : IQuery<PayrollSettingsDto>;

public sealed record PayrollSettingsDto(
    int CutOffStartDay,    // DayOfWeek as int: 0=Sunday, 1=Monday, ..., 6=Saturday
    int Frequency);        // PayrollFrequency as int: 1=Weekly, 2=SemiMonthly
