using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Commands.SuspendFranchisee;

public sealed class SuspendFranchiseeCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<SuspendFranchiseeCommand, Result>
{
    public async Task<Result> Handle(
        SuspendFranchiseeCommand request,
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

        if (agreement.Status == AgreementStatus.Suspended)
            return Result.Failure(Error.Conflict("Franchise agreement is already suspended."));

        agreement.Status = AgreementStatus.Suspended;

        return Result.Success();
    }
}
