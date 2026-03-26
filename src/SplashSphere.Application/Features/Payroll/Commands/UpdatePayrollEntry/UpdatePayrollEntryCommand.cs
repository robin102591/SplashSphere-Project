using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.UpdatePayrollEntry;

/// <summary>
/// Updates the notes on a single payroll entry.
/// Only permitted while the parent period is <c>Closed</c>; rejected for Open or Processed periods.
/// <para>
/// Bonuses and deductions are now managed through itemised <c>PayrollAdjustment</c> rows.
/// </para>
/// </summary>
public sealed record UpdatePayrollEntryCommand(
    string EntryId,
    string? Notes) : ICommand;
