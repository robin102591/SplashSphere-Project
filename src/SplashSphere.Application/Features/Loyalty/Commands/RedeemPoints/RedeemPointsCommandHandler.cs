using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Loyalty.Commands.RedeemPoints;

public sealed class RedeemPointsCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IEventPublisher eventPublisher)
    : IRequestHandler<RedeemPointsCommand, Result<RedemptionResultDto>>
{
    public async Task<Result<RedemptionResultDto>> Handle(
        RedeemPointsCommand request,
        CancellationToken cancellationToken)
    {
        var card = await context.MembershipCards
            .FirstOrDefaultAsync(m => m.Id == request.MembershipCardId, cancellationToken);

        if (card is null)
            return Result.Failure<RedemptionResultDto>(Error.NotFound("MembershipCard", request.MembershipCardId));

        if (!card.IsActive)
            return Result.Failure<RedemptionResultDto>(Error.Validation("Membership card is not active."));

        var reward = await context.LoyaltyRewards
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RewardId && r.IsActive, cancellationToken);

        if (reward is null)
            return Result.Failure<RedemptionResultDto>(Error.NotFound("LoyaltyReward", request.RewardId));

        if (card.PointsBalance < reward.PointsCost)
            return Result.Failure<RedemptionResultDto>(Error.Validation(
                $"Insufficient points. Required: {reward.PointsCost}, Available: {card.PointsBalance}"));

        card.PointsBalance -= reward.PointsCost;
        card.LifetimePointsRedeemed += reward.PointsCost;

        var pointTx = new PointTransaction(
            tenantContext.TenantId,
            card.Id,
            PointTransactionType.Redeemed,
            -reward.PointsCost,
            card.PointsBalance,
            $"Redeemed: {reward.Name}")
        {
            TransactionId = request.TransactionId,
            RewardId = reward.Id,
        };

        context.PointTransactions.Add(pointTx);

        eventPublisher.Enqueue(new Domain.Events.PointsRedeemedEvent(
            card.Id,
            tenantContext.TenantId,
            string.Empty, // branch resolved later if needed
            reward.PointsCost,
            card.PointsBalance,
            reward.Id,
            request.TransactionId ?? string.Empty));

        return Result.Success(new RedemptionResultDto(
            reward.PointsCost,
            card.PointsBalance,
            pointTx.Id));
    }
}
