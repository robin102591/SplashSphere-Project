using MediatR;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Loyalty.Commands.CreateLoyaltyReward;

public sealed class CreateLoyaltyRewardCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<CreateLoyaltyRewardCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateLoyaltyRewardCommand request,
        CancellationToken cancellationToken)
    {
        var reward = new LoyaltyReward(
            tenantContext.TenantId,
            request.Name,
            request.RewardType,
            request.PointsCost)
        {
            Description = request.Description,
            ServiceId = request.ServiceId,
            PackageId = request.PackageId,
            DiscountAmount = request.DiscountAmount,
            DiscountPercent = request.DiscountPercent,
        };

        context.LoyaltyRewards.Add(reward);
        return await Task.FromResult(Result.Success(reward.Id));
    }
}
