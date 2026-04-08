using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Franchise.Commands.UpdateFranchiseSettings;

public sealed record UpdateFranchiseSettingsCommand(
    decimal RoyaltyRate,
    decimal MarketingFeeRate,
    decimal TechnologyFeeRate,
    RoyaltyBasis RoyaltyBasis,
    RoyaltyFrequency RoyaltyFrequency,
    bool EnforceStandardServices,
    bool EnforceStandardPricing,
    bool AllowLocalServices,
    decimal? MaxPriceVariance,
    bool EnforceBranding,
    PlanTier DefaultFranchiseePlan,
    int MaxBranchesPerFranchisee) : ICommand;
