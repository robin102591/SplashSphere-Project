using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Loyalty.Commands.AdjustPoints;

public sealed class AdjustPointsCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<AdjustPointsCommand, Result>
{
    public async Task<Result> Handle(
        AdjustPointsCommand request,
        CancellationToken cancellationToken)
    {
        var card = await context.MembershipCards
            .FirstOrDefaultAsync(m => m.Id == request.MembershipCardId, cancellationToken);

        if (card is null)
            return Result.Failure(Error.NotFound("MembershipCard", request.MembershipCardId));

        card.PointsBalance += request.Points;

        if (request.Points > 0)
            card.LifetimePointsEarned += request.Points;

        // Prevent negative balance
        if (card.PointsBalance < 0)
            card.PointsBalance = 0;

        var pointTx = new PointTransaction(
            tenantContext.TenantId,
            card.Id,
            PointTransactionType.Adjustment,
            request.Points,
            card.PointsBalance,
            $"Admin adjustment: {request.Reason}");

        context.PointTransactions.Add(pointTx);

        return Result.Success();
    }
}
