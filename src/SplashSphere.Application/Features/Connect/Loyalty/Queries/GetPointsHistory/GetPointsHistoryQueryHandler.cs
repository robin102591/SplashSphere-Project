using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Loyalty.Queries.GetPointsHistory;

public sealed class GetPointsHistoryQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<GetPointsHistoryQuery, IReadOnlyList<ConnectPointTransactionDto>>
{
    private const int DefaultTake = 50;
    private const int MaxTake = 200;

    public async Task<IReadOnlyList<ConnectPointTransactionDto>> Handle(
        GetPointsHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated) return [];

        var userId = connectUser.ConnectUserId;

        // Resolve MembershipCardId via link → customer → card at this tenant.
        var cardId = await (
            from link in db.ConnectUserTenantLinks.IgnoreQueryFilters()
            join card in db.MembershipCards.IgnoreQueryFilters()
                on new { link.TenantId, link.CustomerId } equals new { card.TenantId, card.CustomerId }
            where link.ConnectUserId == userId
               && link.TenantId == request.TenantId
               && link.IsActive
               && card.IsActive
            select card.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (cardId is null) return [];

        var take = Math.Clamp(request.Take ?? DefaultTake, 1, MaxTake);

        return await db.PointTransactions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.MembershipCardId == cardId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(take)
            .Select(p => new ConnectPointTransactionDto(
                p.Id,
                p.Type,
                p.Points,
                p.BalanceAfter,
                p.Description,
                p.Reward != null ? p.Reward.Name : null,
                p.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
