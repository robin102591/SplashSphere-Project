using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Loyalty.Commands.UpdateLoyaltyReward;

public sealed class UpdateLoyaltyRewardCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateLoyaltyRewardCommand, Result>
{
    public async Task<Result> Handle(
        UpdateLoyaltyRewardCommand request,
        CancellationToken cancellationToken)
    {
        var reward = await context.LoyaltyRewards
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (reward is null)
            return Result.Failure(Error.NotFound("LoyaltyReward", request.Id));

        reward.Name = request.Name;
        reward.Description = request.Description;
        reward.RewardType = request.RewardType;
        reward.PointsCost = request.PointsCost;
        reward.ServiceId = request.ServiceId;
        reward.PackageId = request.PackageId;
        reward.DiscountAmount = request.DiscountAmount;
        reward.DiscountPercent = request.DiscountPercent;

        return Result.Success();
    }
}
