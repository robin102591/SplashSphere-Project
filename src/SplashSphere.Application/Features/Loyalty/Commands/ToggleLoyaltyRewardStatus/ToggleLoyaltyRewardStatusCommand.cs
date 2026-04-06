using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Loyalty.Commands.ToggleLoyaltyRewardStatus;

public sealed record ToggleLoyaltyRewardStatusCommand(string Id) : ICommand;
