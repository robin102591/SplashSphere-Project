using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.ReopenShift;

/// <summary>Manager reopens a closed shift (only if review is still Pending).</summary>
public sealed record ReopenShiftCommand(string ShiftId) : ICommand;
