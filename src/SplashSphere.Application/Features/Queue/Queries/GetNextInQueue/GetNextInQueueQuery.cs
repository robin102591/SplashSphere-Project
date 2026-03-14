using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Queue.Queries.GetNextInQueue;

/// <summary>
/// Returns the next WAITING entry that would be called — without changing any state.
/// Returns null if no WAITING entries exist.
/// Useful for the POS to preview the next vehicle before the cashier clicks Call.
/// </summary>
public sealed record GetNextInQueueQuery(string BranchId) : IQuery<QueueEntryDto?>;
