using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.ReviewShift;

/// <summary>Manager approves or flags a closed shift.</summary>
public sealed record ReviewShiftCommand(
    string ShiftId,
    ReviewStatus NewReviewStatus,
    string? Notes) : ICommand;
