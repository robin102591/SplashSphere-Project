using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Loyalty.Commands.CreateLoyaltyReward;

public sealed record CreateLoyaltyRewardCommand(
    string Name,
    string? Description,
    RewardType RewardType,
    int PointsCost,
    string? ServiceId,
    string? PackageId,
    decimal? DiscountAmount,
    decimal? DiscountPercent) : ICommand<string>;
