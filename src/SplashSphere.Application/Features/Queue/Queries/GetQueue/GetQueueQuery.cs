using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Queries.GetQueue;

/// <summary>
/// Returns the active queue for a branch, ordered by priority (desc) then CreatedAt (asc).
/// Optionally filtered to specific statuses (defaults to Waiting + Called + InService).
/// </summary>
public sealed record GetQueueQuery(
    string BranchId,
    IReadOnlyList<QueueStatus>? Statuses = null,
    int Page = 1,
    int PageSize = 50) : IQuery<PagedResult<QueueEntryDto>>;
