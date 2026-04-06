using MediatR;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetMembershipCard;

public sealed record GetMembershipCardQuery(string CustomerId) : IRequest<MembershipCardDto?>;
