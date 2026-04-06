using MediatR;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetMembershipCardByNumber;

public sealed record GetMembershipCardByNumberQuery(string CardNumber) : IRequest<MembershipCardDto?>;
