using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Queue.Queries.GetQueueStats;

/// <summary>
/// Real-time queue statistics for a branch.
/// ServedToday counts Completed entries since the start of the Asia/Manila calendar day.
/// AvgWaitMinutes is the average elapsed time from CalledAt to StartedAt for today's
/// InService/Completed entries.
/// </summary>
public sealed record GetQueueStatsQuery(string BranchId) : IQuery<QueueStatsDto>;
