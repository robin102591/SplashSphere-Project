using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetPointHistory;

public sealed class GetPointHistoryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPointHistoryQuery, PagedResult<PointTransactionDto>>
{
    public async Task<PagedResult<PointTransactionDto>> Handle(
        GetPointHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.PointTransactions
            .AsNoTracking()
            .Where(p => p.MembershipCardId == request.MembershipCardId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PointTransactionDto(
                p.Id,
                p.Type,
                p.Points,
                p.BalanceAfter,
                p.Description,
                p.TransactionId,
                p.Reward != null ? p.Reward.Name : null,
                p.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<PointTransactionDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
