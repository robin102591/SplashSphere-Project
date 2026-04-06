using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Loyalty.Commands.ToggleLoyaltyRewardStatus;

public sealed class ToggleLoyaltyRewardStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleLoyaltyRewardStatusCommand, Result>
{
    public async Task<Result> Handle(
        ToggleLoyaltyRewardStatusCommand request,
        CancellationToken cancellationToken)
    {
        var reward = await context.LoyaltyRewards
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (reward is null)
            return Result.Failure(Error.NotFound("LoyaltyReward", request.Id));

        reward.IsActive = !reward.IsActive;
        return Result.Success();
    }
}
