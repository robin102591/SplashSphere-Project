using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Franchise;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.Franchise.Commands.ValidateInvitation;

public sealed class ValidateInvitationQueryHandler(IApplicationDbContext context)
    : IRequestHandler<ValidateInvitationQuery, InvitationDetailsDto>
{
    public async Task<InvitationDetailsDto> Handle(
        ValidateInvitationQuery request,
        CancellationToken cancellationToken)
    {
        var invitation = await context.FranchiseInvitations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(i => i.FranchisorTenant)
            .FirstOrDefaultAsync(i => i.Token == request.Token, cancellationToken)
            ?? throw new NotFoundException($"Invitation with token '{request.Token}' was not found.");

        if (invitation.IsUsed)
            throw new ValidationException("This invitation has already been used.");

        if (invitation.ExpiresAt <= DateTime.UtcNow)
            throw new ValidationException("This invitation has expired.");

        return new InvitationDetailsDto(
            invitation.FranchisorTenant.Name,
            invitation.BusinessName,
            invitation.Email,
            invitation.FranchiseCode,
            invitation.TerritoryName,
            invitation.ExpiresAt);
    }
}
