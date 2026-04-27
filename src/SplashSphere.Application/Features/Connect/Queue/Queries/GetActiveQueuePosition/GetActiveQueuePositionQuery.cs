using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Queue.Queries.GetActiveQueuePosition;

/// <summary>
/// Returns the caller's currently active queue entry across all tenants, or
/// <c>null</c> if they have none. "Active" means the entry is not yet in a
/// terminal state (<c>Completed</c> / <c>Cancelled</c> / <c>NoShow</c>).
/// </summary>
public sealed record GetActiveQueuePositionQuery : IQuery<ConnectActiveQueueDto?>;
