using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Referral.Queries.GetMyReferrals;

public sealed class GetMyReferralsQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<GetMyReferralsQuery, IReadOnlyList<ConnectReferralListItemDto>>
{
    public async Task<IReadOnlyList<ConnectReferralListItemDto>> Handle(
        GetMyReferralsQuery request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated) return [];

        var userId = connectUser.ConnectUserId;

        var customerId = await db.ConnectUserTenantLinks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(l => l.ConnectUserId == userId
                     && l.TenantId == request.TenantId
                     && l.IsActive)
            .Select(l => l.CustomerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (customerId is null) return [];

        return await (
            from r in db.Referrals.IgnoreQueryFilters()
            join c in db.Customers.IgnoreQueryFilters()
                on r.ReferredCustomerId equals c.Id into referredJoin
            from c in referredJoin.DefaultIfEmpty()
            where r.TenantId == request.TenantId
               && r.ReferrerCustomerId == customerId
               && r.ReferredCustomerId != null
            orderby r.CreatedAt descending
            select new ConnectReferralListItemDto(
                r.Id,
                c == null ? null : (c.FirstName + " " + c.LastName).Trim(),
                r.Status,
                r.ReferrerPointsEarned,
                r.CompletedAt,
                r.CreatedAt))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
