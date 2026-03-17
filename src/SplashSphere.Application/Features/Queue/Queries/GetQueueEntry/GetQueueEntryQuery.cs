using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Queries.GetQueueEntry;

/// <summary>
/// Returns a single queue entry by ID.
/// Used by the POS new-transaction page to pre-fill vehicle and service data
/// when navigating from the queue board via <c>?queueEntryId=xxx</c>.
/// </summary>
public sealed record GetQueueEntryQuery(string QueueEntryId) : IQuery<Result<QueueEntryDto>>;
