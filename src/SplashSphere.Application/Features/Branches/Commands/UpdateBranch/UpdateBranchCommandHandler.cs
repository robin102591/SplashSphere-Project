using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Branches.Commands.UpdateBranch;

public sealed class UpdateBranchCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateBranchCommand, Result>
{
    public async Task<Result> Handle(
        UpdateBranchCommand request,
        CancellationToken cancellationToken)
    {
        // Tracked fetch — EF Core detects property changes automatically.
        var branch = await context.Branches
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (branch is null)
            return Result.Failure(Error.NotFound("Branch", request.Id));

        var normalizedCode = request.Code.ToUpperInvariant();

        var codeConflict = await context.Branches
            .AnyAsync(b => b.Code == normalizedCode && b.Id != request.Id, cancellationToken);

        if (codeConflict)
            return Result.Failure(
                Error.Conflict($"Branch code '{normalizedCode}' is already in use."));

        branch.Name          = request.Name;
        branch.Code          = normalizedCode;
        branch.Address       = request.Address;
        branch.ContactNumber = request.ContactNumber;

        // UnitOfWorkBehavior calls SaveChangesAsync — tracked entity picked up automatically.
        return Result.Success();
    }
}
