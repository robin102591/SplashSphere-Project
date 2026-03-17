using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Queue.Queries.GetQueueDisplay;

/// <summary>
/// Public queue display data for the wall-mounted TV screen.
/// No tenant auth required — accessed via <c>GET /queue/display?branchId=xxx</c>.
/// Returns Called and InService entries so the TV shows whose turn it is.
/// Plates are masked (first 3 + *** + last 2).
/// </summary>
public sealed record GetQueueDisplayQuery(string BranchId) : IQuery<QueueDisplaySnapshotDto>;
