using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.ProcessPayrollPeriod;

/// <summary>
/// Transitions a Closed payroll period to Processed, making it fully immutable.
/// No further adjustments to entries are permitted after this point.
/// Publishes <c>PayrollProcessedEvent</c> on success.
/// </summary>
public sealed record ProcessPayrollPeriodCommand(string PeriodId) : ICommand;
