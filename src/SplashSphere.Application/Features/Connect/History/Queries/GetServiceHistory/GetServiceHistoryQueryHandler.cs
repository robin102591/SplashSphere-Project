using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Connect.History.Queries.GetServiceHistory;

public sealed class GetServiceHistoryQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<GetServiceHistoryQuery, IReadOnlyList<ConnectServiceHistoryItemDto>>
{
    private const int DefaultTake = 50;
    private const int MaxTake = 200;

    public async Task<IReadOnlyList<ConnectServiceHistoryItemDto>> Handle(
        GetServiceHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated) return [];

        var userId = connectUser.ConnectUserId;

        var customerIds = await db.ConnectUserTenantLinks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(l => l.ConnectUserId == userId && l.IsActive)
            .Select(l => l.CustomerId)
            .ToListAsync(cancellationToken);

        if (customerIds.Count == 0) return [];

        var take = Math.Clamp(request.Take ?? DefaultTake, 1, MaxTake);

        var rows = await (
            from tx in db.Transactions.IgnoreQueryFilters()
            join t in db.Tenants.IgnoreQueryFilters() on tx.TenantId equals t.Id
            join br in db.Branches.IgnoreQueryFilters() on tx.BranchId equals br.Id
            join car in db.Cars.IgnoreQueryFilters() on tx.CarId equals car.Id
            where tx.CustomerId != null
               && customerIds.Contains(tx.CustomerId)
               && tx.Status == TransactionStatus.Completed
            orderby tx.CompletedAt descending
            select new
            {
                tx.Id,
                tx.TransactionNumber,
                tx.TenantId,
                TenantName = t.Name,
                tx.BranchId,
                BranchName = br.Name,
                Plate = car.PlateNumber,
                tx.FinalAmount,
                tx.PointsEarned,
                tx.CompletedAt,
            })
            .AsNoTracking()
            .Take(take)
            .ToListAsync(cancellationToken);

        if (rows.Count == 0) return [];

        var txIds = rows.Select(r => r.Id).ToList();

        var serviceNamesByTx = await (
            from ts in db.TransactionServices.IgnoreQueryFilters()
            join s in db.Services.IgnoreQueryFilters() on ts.ServiceId equals s.Id
            where txIds.Contains(ts.TransactionId)
            select new { ts.TransactionId, s.Name })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var lookup = serviceNamesByTx
            .GroupBy(x => x.TransactionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(x => x.Name).ToList());

        return rows
            .Select(r => new ConnectServiceHistoryItemDto(
                TransactionId: r.Id,
                TransactionNumber: r.TransactionNumber,
                TenantId: r.TenantId,
                TenantName: r.TenantName,
                BranchId: r.BranchId,
                BranchName: r.BranchName,
                PlateNumber: r.Plate,
                FinalAmount: r.FinalAmount,
                PointsEarned: r.PointsEarned,
                CompletedAt: r.CompletedAt ?? DateTime.MinValue,
                ServiceNames: lookup.TryGetValue(r.Id, out var names) ? names : []))
            .ToList();
    }
}
