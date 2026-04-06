using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetMembershipCard;

public sealed class GetMembershipCardQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMembershipCardQuery, MembershipCardDto?>
{
    public async Task<MembershipCardDto?> Handle(
        GetMembershipCardQuery request,
        CancellationToken cancellationToken)
    {
        return await context.MembershipCards
            .AsNoTracking()
            .Where(m => m.CustomerId == request.CustomerId)
            .Select(m => new MembershipCardDto(
                m.Id,
                m.CustomerId,
                m.Customer.FirstName + " " + m.Customer.LastName,
                m.Customer.Email,
                m.Customer.ContactNumber,
                m.CardNumber,
                m.CurrentTier,
                m.CurrentTier.ToString(),
                m.PointsBalance,
                m.LifetimePointsEarned,
                m.LifetimePointsRedeemed,
                m.IsActive,
                m.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
