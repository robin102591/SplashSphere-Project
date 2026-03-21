using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.CloseShift;

/// <summary>
/// Closes the shift: queries transactions, computes totals, saves denomination count,
/// calculates variance, and sets auto-review status.
/// </summary>
public sealed record CloseShiftCommand(
    string ShiftId,
    IReadOnlyList<DenominationEntry> Denominations) : ICommand;

/// <summary>A single denomination in the physical cash count.</summary>
public sealed record DenominationEntry(decimal DenominationValue, int Count);
