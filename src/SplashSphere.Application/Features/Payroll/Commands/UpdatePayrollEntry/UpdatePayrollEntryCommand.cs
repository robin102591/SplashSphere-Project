using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollEntry;

/// <summary>
/// Adjusts the bonus and deduction amounts on a single payroll entry.
/// Only permitted while the parent period is <c>Closed</c>; rejected for Open or Processed periods.
/// </summary>
public sealed record UpdatePayrollEntryCommand(
    string EntryId,
    decimal Bonuses,
    decimal Deductions,
    string? Notes) : ICommand;
