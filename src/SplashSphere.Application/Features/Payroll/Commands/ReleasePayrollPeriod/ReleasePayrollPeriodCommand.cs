using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Commands.ReleasePayrollPeriod;

/// <summary>
/// Transitions a Processed payroll period to Released, marking pay as disbursed.
/// Terminal state — no further changes permitted.
/// Publishes <c>PayrollReleasedEvent</c> on success.
/// </summary>
public sealed record ReleasePayrollPeriodCommand(string PeriodId) : ICommand;
