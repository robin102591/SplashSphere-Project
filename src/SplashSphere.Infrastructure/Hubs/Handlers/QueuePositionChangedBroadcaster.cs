using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.Infrastructure.Services;

namespace SplashSphere.Infrastructure.Hubs.Handlers;

/// <summary>
/// Listens to queue state changes (Created / Called / InService / NoShow) and, for
/// each remaining <see cref="QueueStatus.Waiting"/> entry at the branch, persists
/// a lightweight <see cref="NotificationType.QueuePositionChanged"/> record so
/// future customer-facing polling endpoints can surface updated wait positions.
/// <para>
/// Scope is deliberately narrow: one notification per affected entry. The Connect
/// frontend (Phase 22.3) will poll and dedupe; push delivery is out of scope.
/// </para>
/// </summary>
public sealed class QueuePositionChangedBroadcaster(
    IApplicationDbContext db,
    INotificationService notifications)
    : INotificationHandler<DomainEventNotification<QueueEntryCreatedEvent>>,
      INotificationHandler<DomainEventNotification<QueueEntryCalledEvent>>,
      INotificationHandler<DomainEventNotification<QueueEntryInServiceEvent>>,
      INotificationHandler<DomainEventNotification<QueueEntryNoShowEvent>>
{
    public Task Handle(DomainEventNotification<QueueEntryCreatedEvent> n, CancellationToken ct)
        => BroadcastAsync(n.Event.TenantId, n.Event.BranchId, ct);

    public Task Handle(DomainEventNotification<QueueEntryCalledEvent> n, CancellationToken ct)
        => BroadcastAsync(n.Event.TenantId, n.Event.BranchId, ct);

    public Task Handle(DomainEventNotification<QueueEntryInServiceEvent> n, CancellationToken ct)
        => BroadcastAsync(n.Event.TenantId, n.Event.BranchId, ct);

    public Task Handle(DomainEventNotification<QueueEntryNoShowEvent> n, CancellationToken ct)
        => BroadcastAsync(n.Event.TenantId, n.Event.BranchId, ct);

    private async Task BroadcastAsync(string tenantId, string branchId, CancellationToken ct)
    {
        // Rank Waiting entries at the branch by priority desc, then CreatedAt asc.
        // Priority order: Vip (4) > Booked (3) > Express (2) > Regular (1).
        var waiting = await db.QueueEntries
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(q => q.TenantId == tenantId
                     && q.BranchId == branchId
                     && q.Status == QueueStatus.Waiting)
            .OrderByDescending(q => q.Priority)
            .ThenBy(q => q.CreatedAt)
            .Select(q => new { q.Id, q.QueueNumber, q.PlateNumber })
            .ToListAsync(ct);

        if (waiting.Count == 0) return;

        for (var i = 0; i < waiting.Count; i++)
        {
            var entry = waiting[i];
            var positionAhead = i;

            await notifications.SendAsync(new SendNotificationRequest
            {
                TenantId = tenantId,
                Type = NotificationType.QueuePositionChanged,
                Title = $"Queue position — {entry.QueueNumber}",
                Message = positionAhead == 0
                    ? $"{entry.QueueNumber} ({entry.PlateNumber}) is next in line."
                    : $"{entry.QueueNumber} ({entry.PlateNumber}) has {positionAhead} ahead.",
                ReferenceId = entry.Id,
                ReferenceType = "QueueEntry",
            }, ct);
        }
    }
}
