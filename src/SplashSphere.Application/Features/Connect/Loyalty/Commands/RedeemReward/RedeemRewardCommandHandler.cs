using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.Domain.Subscription;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Loyalty.Commands.RedeemReward;

public sealed class RedeemRewardCommandHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser,
    IPlanEnforcementService planService,
    IEventPublisher eventPublisher)
    : IRequestHandler<RedeemRewardCommand, Result<ConnectRedemptionResultDto>>
{
    public async Task<Result<ConnectRedemptionResultDto>> Handle(
        RedeemRewardCommand request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated)
            return Result.Failure<ConnectRedemptionResultDto>(Error.Unauthorized("Sign in required."));

        var hasFeature = await planService.HasFeatureAsync(
            request.TenantId, FeatureKeys.CustomerLoyalty, cancellationToken);
        if (!hasFeature)
            return Result.Failure<ConnectRedemptionResultDto>(
                Error.Forbidden("This car wash does not offer a loyalty program."));

        var userId = connectUser.ConnectUserId;

        // Link required.
        var link = await db.ConnectUserTenantLinks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                l => l.ConnectUserId == userId
                  && l.TenantId == request.TenantId
                  && l.IsActive,
                cancellationToken);
        if (link is null)
            return Result.Failure<ConnectRedemptionResultDto>(
                Error.Forbidden("Join this car wash before redeeming."));

        var card = await db.MembershipCards
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                m => m.TenantId == request.TenantId && m.CustomerId == link.CustomerId,
                cancellationToken);
        if (card is null || !card.IsActive)
            return Result.Failure<ConnectRedemptionResultDto>(
                Error.Validation("You are not enrolled in this car wash's loyalty program yet."));

        var reward = await db.LoyaltyRewards
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.Id == request.RewardId
                  && r.TenantId == request.TenantId
                  && r.IsActive,
                cancellationToken);
        if (reward is null)
            return Result.Failure<ConnectRedemptionResultDto>(
                Error.NotFound("LoyaltyReward", request.RewardId));

        if (card.PointsBalance < reward.PointsCost)
            return Result.Failure<ConnectRedemptionResultDto>(Error.Validation(
                $"Insufficient points. Required: {reward.PointsCost}, Available: {card.PointsBalance}."));

        card.PointsBalance -= reward.PointsCost;
        card.LifetimePointsRedeemed += reward.PointsCost;

        var pointTx = new PointTransaction(
            tenantId: request.TenantId,
            membershipCardId: card.Id,
            type: PointTransactionType.Redeemed,
            points: -reward.PointsCost,
            balanceAfter: card.PointsBalance,
            description: $"Redeemed: {reward.Name}")
        {
            RewardId = reward.Id,
        };

        db.PointTransactions.Add(pointTx);

        eventPublisher.Enqueue(new PointsRedeemedEvent(
            card.Id,
            request.TenantId,
            BranchId: string.Empty, // Connect redemptions are tenant-level, branch resolved at POS claim.
            reward.PointsCost,
            card.PointsBalance,
            reward.Id,
            TransactionId: string.Empty));

        return Result.Success(new ConnectRedemptionResultDto(
            PointTransactionId: pointTx.Id,
            RewardId: reward.Id,
            RewardName: reward.Name,
            PointsDeducted: reward.PointsCost,
            NewBalance: card.PointsBalance));
    }
}
