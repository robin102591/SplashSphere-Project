using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Commands.CreateFranchiseAgreement;

public sealed record CreateFranchiseAgreementCommand(
    string FranchiseeTenantId,
    string AgreementNumber,
    string TerritoryName,
    string? TerritoryDescription,
    bool ExclusiveTerritory,
    DateTime StartDate,
    DateTime? EndDate,
    decimal InitialFranchiseFee,
    decimal? CustomRoyaltyRate,
    decimal? CustomMarketingFeeRate,
    string? Notes) : ICommand<string>;
