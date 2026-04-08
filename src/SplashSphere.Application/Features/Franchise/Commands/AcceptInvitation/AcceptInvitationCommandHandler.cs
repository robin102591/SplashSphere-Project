using MediatR;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Commands.AcceptInvitation;

public sealed class AcceptInvitationCommandHandler
    : IRequestHandler<AcceptInvitationCommand, Result<string>>
{
    public Task<Result<string>> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            Result.Failure<string>(Error.Domain("Accept invitation flow not yet implemented.")));
    }
}
