using MediatR;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Commands.InviteFranchisee;

public sealed class InviteFranchiseeCommandHandler
    : IRequestHandler<InviteFranchiseeCommand, Result<string>>
{
    public Task<Result<string>> Handle(
        InviteFranchiseeCommand request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            Result.Failure<string>(Error.Domain("Invitation flow not yet implemented.")));
    }
}
