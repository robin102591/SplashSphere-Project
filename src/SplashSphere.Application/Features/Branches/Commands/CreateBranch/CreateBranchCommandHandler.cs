using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Branches.Commands.CreateBranch;

public sealed class CreateBranchCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IPlanEnforcementService planService)
    : IRequestHandler<CreateBranchCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CreateBranchCommand request,
        CancellationToken cancellationToken)
    {
        // ── Plan limit check ─────────────────────────────────────────────────
        var limitCheck = await planService.CheckLimitAsync(
            tenantContext.TenantId, LimitType.Branches, cancellationToken);
        if (!limitCheck.Allowed)
            return Result.Failure<string>(Error.Validation(limitCheck.Message));

        var codeExists = await context.Branches
            .AnyAsync(b => b.Code == request.Code.ToUpperInvariant(), cancellationToken);

        if (codeExists)
            return Result.Failure<string>(
                Error.Conflict($"Branch code '{request.Code.ToUpperInvariant()}' is already in use."));

        var branch = new Branch(
            tenantContext.TenantId,
            request.Name,
            request.Code,
            request.Address,
            request.ContactNumber);

        context.Branches.Add(branch);

        // UnitOfWorkBehavior calls SaveChangesAsync after this handler returns.
        return Result.Success(branch.Id);
    }
}
