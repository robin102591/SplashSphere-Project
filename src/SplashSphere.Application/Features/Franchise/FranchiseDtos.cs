using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Franchise;

public sealed record FranchiseSettingsDto(
    string Id,
    string TenantId,
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
    int MaxBranchesPerFranchisee);

public sealed record FranchiseeListItemDto(
    string TenantId,
    string Name,
    string? FranchiseCode,
    string TerritoryName,
    int BranchCount,
    bool IsActive,
    AgreementStatus AgreementStatus,
    decimal RevenueThisMonth,
    decimal RoyaltyDue);

public sealed record FranchiseeDetailDto(
    string TenantId,
    string Name,
    string? FranchiseCode,
    string Email,
    string ContactNumber,
    string Address,
    string TerritoryName,
    int BranchCount,
    bool IsActive,
    AgreementStatus AgreementStatus,
    decimal RevenueThisMonth,
    decimal RoyaltyDue,
    FranchiseAgreementDto? Agreement,
    IReadOnlyList<RoyaltyPeriodDto> RecentRoyalties);

public sealed record FranchiseAgreementDto(
    string Id,
    string FranchisorTenantId,
    string FranchiseeTenantId,
    string AgreementNumber,
    string TerritoryName,
    string? TerritoryDescription,
    bool ExclusiveTerritory,
    DateTime StartDate,
    DateTime? EndDate,
    decimal InitialFranchiseFee,
    AgreementStatus Status,
    decimal? CustomRoyaltyRate,
    decimal? CustomMarketingFeeRate,
    string? Notes,
    DateTime CreatedAt);

public sealed record RoyaltyPeriodDto(
    string Id,
    string FranchiseeTenantId,
    string FranchiseeName,
    string AgreementId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal GrossRevenue,
    decimal RoyaltyRate,
    decimal RoyaltyAmount,
    decimal MarketingFeeRate,
    decimal MarketingFeeAmount,
    decimal TechnologyFeeRate,
    decimal TechnologyFeeAmount,
    decimal TotalDue,
    RoyaltyStatus Status,
    DateTime? PaidDate,
    string? PaymentReference);

public sealed record NetworkSummaryDto(
    int TotalFranchisees,
    int ActiveFranchisees,
    int SuspendedFranchisees,
    int PendingFranchisees,
    decimal NetworkRevenueThisMonth,
    decimal TotalRoyaltiesCollected,
    decimal PendingRoyalties,
    decimal OverdueRoyalties,
    decimal AverageRevenuePerFranchisee);

public sealed record FranchiseComplianceItemDto(
    string TenantId,
    string Name,
    string TerritoryName,
    bool UsingStandardServices,
    bool PricingCompliant,
    bool RoyaltiesCurrent,
    bool AgreementExpiringSoon,
    int ComplianceScore);

public sealed record FranchiseBenchmarkDto(
    string Metric,
    decimal YourValue,
    decimal NetworkAverage,
    int Rank,
    int TotalInNetwork);

public sealed record FranchiseServiceTemplateDto(
    string Id,
    string ServiceName,
    string? Description,
    string? CategoryName,
    decimal BasePrice,
    int DurationMinutes,
    bool IsRequired,
    bool IsActive);

public sealed record InvitationDetailsDto(
    string FranchisorName,
    string BusinessName,
    string Email,
    string? FranchiseCode,
    string? TerritoryName,
    DateTime ExpiresAt);
