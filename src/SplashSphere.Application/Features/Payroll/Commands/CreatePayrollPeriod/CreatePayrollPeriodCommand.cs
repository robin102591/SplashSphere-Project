using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Commands.CreatePayrollPeriod;

/// <summary>
/// Manually creates a payroll period for a specific date range.
/// The period covers StartDate through EndDate (inclusive, 7 days).
/// </summary>
public sealed record CreatePayrollPeriodCommand(
    DateOnly StartDate,
    DateOnly EndDate) : ICommand<string>;
