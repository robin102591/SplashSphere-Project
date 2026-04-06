using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Loyalty.Commands.UpsertLoyaltyTiers;

public sealed record UpsertLoyaltyTiersCommand(
    IReadOnlyList<TierInput> Tiers) : ICommand;

public sealed record TierInput(
    LoyaltyTier Tier,
    string Name,
    int MinimumLifetimePoints,
    decimal PointsMultiplier);
