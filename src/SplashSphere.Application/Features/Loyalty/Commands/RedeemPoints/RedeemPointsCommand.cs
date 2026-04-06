using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Loyalty.Commands.RedeemPoints;

/// <summary>
/// Redeems a loyalty reward for a customer. Returns the redemption details
/// (discount amount or free service info) to be applied to the transaction.
/// </summary>
public sealed record RedeemPointsCommand(
    string MembershipCardId,
    string RewardId,
    string? TransactionId) : ICommand<RedemptionResultDto>;

public sealed record RedemptionResultDto(
    int PointsDeducted,
    int NewBalance,
    string PointTransactionId);
