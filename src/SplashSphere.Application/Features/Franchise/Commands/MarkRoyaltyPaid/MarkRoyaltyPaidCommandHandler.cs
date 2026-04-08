using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Commands.MarkRoyaltyPaid;

public sealed class MarkRoyaltyPaidCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<MarkRoyaltyPaidCommand, Result>
{
    public async Task<Result> Handle(
        MarkRoyaltyPaidCommand request,
        CancellationToken cancellationToken)
    {
        var royaltyPeriod = await db.RoyaltyPeriods
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                rp => rp.Id == request.RoyaltyPeriodId
                      && rp.FranchisorTenantId == tenantContext.TenantId,
                cancellationToken);

        if (royaltyPeriod is null)
            return Result.Failure(Error.NotFound("RoyaltyPeriod", request.RoyaltyPeriodId));

        if (royaltyPeriod.Status == RoyaltyStatus.Paid)
            return Result.Failure(Error.Conflict("Royalty period is already marked as paid."));

        royaltyPeriod.Status = RoyaltyStatus.Paid;
        royaltyPeriod.PaidDate = DateTime.UtcNow;
        royaltyPeriod.PaymentReference = request.PaymentReference;

        return Result.Success();
    }
}
