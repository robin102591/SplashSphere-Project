using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Connect.Queue.Queries.GetActiveQueuePosition;

public sealed class GetActiveQueuePositionQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser)
    : IRequestHandler<GetActiveQueuePositionQuery, ConnectActiveQueueDto?>
{
    private static readonly QueueStatus[] ActiveStatuses =
    [
        QueueStatus.Waiting,
        QueueStatus.Called,
        QueueStatus.InService,
    ];

    public async Task<ConnectActiveQueueDto?> Handle(
        GetActiveQueuePositionQuery request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated) return null;

        var userId = connectUser.ConnectUserId;

        // Caller → customers at every tenant they've joined.
        var customerIds = await db.ConnectUserTenantLinks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(l => l.ConnectUserId == userId && l.IsActive)
            .Select(l => l.CustomerId)
            .ToListAsync(cancellationToken);

        if (customerIds.Count == 0) return null;

        // Most-recent active queue entry tied to any of those customers.
        var active = await (
            from q in db.QueueEntries.IgnoreQueryFilters()
            join t in db.Tenants.IgnoreQueryFilters() on q.TenantId equals t.Id
            join br in db.Branches.IgnoreQueryFilters() on q.BranchId equals br.Id
            join b in db.Bookings.IgnoreQueryFilters() on q.Id equals b.QueueEntryId into bookingJoin
            from b in bookingJoin.DefaultIfEmpty()
            where q.CustomerId != null
               && customerIds.Contains(q.CustomerId!)
               && ActiveStatuses.Contains(q.Status)
            orderby q.CreatedAt descending
            select new
            {
                Entry = q,
                TenantName = t.Name,
                BranchName = br.Name,
                BookingId = b == null ? null : b.Id,
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (active is null) return null;

        // "Ahead of me" — count of Waiting entries at the same branch with
        // higher priority OR same priority but earlier CreatedAt.
        int? aheadCount = null;
        if (active.Entry.Status == QueueStatus.Waiting)
        {
            aheadCount = await db.QueueEntries
                .IgnoreQueryFilters()
                .AsNoTracking()
                .CountAsync(q => q.BranchId == active.Entry.BranchId
                              && q.Status == QueueStatus.Waiting
                              && (q.Priority > active.Entry.Priority
                                  || (q.Priority == active.Entry.Priority
                                      && q.CreatedAt < active.Entry.CreatedAt)),
                            cancellationToken);
        }
        else if (active.Entry.Status == QueueStatus.Called)
        {
            // Called is up next — effectively 0 ahead.
            aheadCount = 0;
        }

        return new ConnectActiveQueueDto(
            QueueEntryId: active.Entry.Id,
            TenantId: active.Entry.TenantId,
            TenantName: active.TenantName,
            BranchId: active.Entry.BranchId,
            BranchName: active.BranchName,
            QueueNumber: active.Entry.QueueNumber,
            Status: active.Entry.Status,
            Priority: active.Entry.Priority,
            AheadCount: aheadCount,
            EstimatedWaitMinutes: active.Entry.EstimatedWaitMinutes,
            CalledAt: active.Entry.CalledAt,
            StartedAt: active.Entry.StartedAt,
            BookingId: active.BookingId);
    }
}
