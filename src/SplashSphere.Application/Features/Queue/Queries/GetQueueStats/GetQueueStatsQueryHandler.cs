using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Queue.Queries.GetQueueStats;

public sealed class GetQueueStatsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetQueueStatsQuery, QueueStatsDto>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<QueueStatsDto> Handle(
        GetQueueStatsQuery request,
        CancellationToken cancellationToken)
    {
        var localToday = DateOnly.FromDateTime(DateTime.UtcNow + ManilaOffset);
        var todayStartUtc = localToday.ToDateTime(TimeOnly.MinValue);

        var entries = await context.QueueEntries
            .AsNoTracking()
            .Where(q => q.BranchId == request.BranchId && q.CreatedAt >= todayStartUtc)
            .Select(q => new { q.Status, q.CalledAt, q.StartedAt })
            .ToListAsync(cancellationToken);

        var waitingCount   = entries.Count(e => e.Status == QueueStatus.Waiting);
        var calledCount    = entries.Count(e => e.Status == QueueStatus.Called);
        var inServiceCount = entries.Count(e => e.Status == QueueStatus.InService);
        var servedToday    = entries.Count(e => e.Status == QueueStatus.Completed);

        // Average wait = mean of (StartedAt - CalledAt) for entries where both are set.
        var waitSamples = entries
            .Where(e => e.CalledAt.HasValue && e.StartedAt.HasValue)
            .Select(e => (e.StartedAt!.Value - e.CalledAt!.Value).TotalMinutes)
            .ToList();

        double? avgWait = waitSamples.Count > 0 ? waitSamples.Average() : null;

        return new QueueStatsDto(waitingCount, calledCount, inServiceCount, servedToday, avgWait);
    }
}
