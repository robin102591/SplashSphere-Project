using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Branches.Commands.ToggleBranchStatus;

public sealed class ToggleBranchStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleBranchStatusCommand, Result>
{
    public async Task<Result> Handle(
        ToggleBranchStatusCommand request,
        CancellationToken cancellationToken)
    {
        var branch = await context.Branches
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (branch is null)
            return Result.Failure(Error.NotFound("Branch", request.Id));

        branch.IsActive = !branch.IsActive;

        return Result.Success();
    }
}
