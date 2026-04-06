using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetMembershipCardByNumber;

public sealed class GetMembershipCardByNumberQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMembershipCardByNumberQuery, MembershipCardDto?>
{
    public async Task<MembershipCardDto?> Handle(
        GetMembershipCardByNumberQuery request,
        CancellationToken cancellationToken)
    {
        return await context.MembershipCards
            .AsNoTracking()
            .IgnoreQueryFilters() // Card lookup crosses tenants (QR scan)
            .Where(m => m.CardNumber == request.CardNumber && m.IsActive)
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
