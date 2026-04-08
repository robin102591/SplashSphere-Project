using MediatR;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.Franchise.Commands.ValidateInvitation;

public sealed class ValidateInvitationQueryHandler
    : IRequestHandler<ValidateInvitationQuery, InvitationDetailsDto>
{
    public Task<InvitationDetailsDto> Handle(
        ValidateInvitationQuery request,
        CancellationToken cancellationToken)
    {
        throw new NotFoundException(
            $"Invitation with token '{request.Token}' was not found. Invitation flow not yet implemented.");
    }
}
