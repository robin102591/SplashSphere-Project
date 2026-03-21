using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.VoidShift;

/// <summary>Manager voids a shift — only allowed if no transactions were processed during it.</summary>
public sealed record VoidShiftCommand(string ShiftId, string Reason) : ICommand;
