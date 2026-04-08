using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Commands.ReactivateFranchisee;

public sealed class ReactivateFranchiseeCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<ReactivateFranchiseeCommand, Result>
{
    public async Task<Result> Handle(
        ReactivateFranchiseeCommand request,
        CancellationToken cancellationToken)
    {
        var agreement = await db.FranchiseAgreements
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                a => a.FranchisorTenantId == tenantContext.TenantId
                     && a.FranchiseeTenantId == request.FranchiseeTenantId,
                cancellationToken);

        if (agreement is null)
            return Result.Failure(Error.NotFound("FranchiseAgreement", request.FranchiseeTenantId));

        if (agreement.Status != AgreementStatus.Suspended)
            return Result.Failure(Error.Conflict("Only suspended agreements can be reactivated."));

        agreement.Status = AgreementStatus.Active;

        return Result.Success();
    }
}
