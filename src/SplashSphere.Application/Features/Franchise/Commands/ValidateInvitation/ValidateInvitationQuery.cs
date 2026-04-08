using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Commands.ValidateInvitation;

public sealed record ValidateInvitationQuery(string Token) : IQuery<InvitationDetailsDto>;
