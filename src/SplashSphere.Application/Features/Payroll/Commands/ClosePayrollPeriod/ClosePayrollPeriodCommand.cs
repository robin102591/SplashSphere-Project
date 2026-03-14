using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.ClosePayrollPeriod;

/// <summary>
/// Closes an Open payroll period. Creates a <c>PayrollEntry</c> for every active
/// employee in the tenant who has attendance days or earned commissions during the period.
/// Publishes <c>PayrollPeriodClosedEvent</c> on success.
/// </summary>
public sealed record ClosePayrollPeriodCommand(string PeriodId) : ICommand;
