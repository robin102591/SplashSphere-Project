using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Commands.CallNextInQueue;

public sealed class CallNextInQueueCommandHandler(
    IApplicationDbContext context,
    IEventPublisher eventPublisher,
    IBackgroundJobService backgroundJobs)
    : IRequestHandler<CallNextInQueueCommand, Result<string?>>
{
    private static readonly TimeSpan NoShowTimeout = TimeSpan.FromMinutes(5);

    public async Task<Result<string?>> Handle(
        CallNextInQueueCommand request,
        CancellationToken cancellationToken)
    {
        // Pick highest priority WAITING entry; FIFO within the same priority tier.
        // QueuePriority: Vip=3 > Express=2 > Regular=1 — order descending by value.
        var entry = await context.QueueEntries
            .Where(q => q.BranchId == request.BranchId && q.Status == QueueStatus.Waiting)
            .OrderByDescending(q => q.Priority)
            .ThenBy(q => q.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (entry is null)
            return Result.Success<string?>(null); // No one waiting — not an error.

        var now = DateTime.UtcNow;
        entry.Status   = QueueStatus.Called;
        entry.CalledAt = now;

        // Schedule the 5-minute no-show timer. The job fires MarkNoShowCommand which
        // is idempotent — it skips if the entry is no longer in Called state.
        backgroundJobs.ScheduleNoShowCheck(entry.Id, NoShowTimeout);

        eventPublisher.Enqueue(new QueueEntryCalledEvent(
            entry.Id,
            entry.TenantId,
            request.BranchId,
            entry.QueueNumber,
            entry.PlateNumber,
            now));

        return Result.Success<string?>(entry.Id);
    }
}
