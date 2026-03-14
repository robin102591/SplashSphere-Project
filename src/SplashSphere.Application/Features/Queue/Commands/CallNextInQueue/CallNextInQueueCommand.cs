using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Commands.CallNextInQueue;

/// <summary>
/// Calls the next highest-priority WAITING entry to a service bay.
/// Selection: highest <see cref="Domain.Enums.QueuePriority"/> value, then earliest CreatedAt (FIFO).
/// Returns the QueueEntry ID of the called entry, or null if no WAITING entries exist.
/// Schedules the 5-minute no-show timer via <see cref="IBackgroundJobService"/>.
/// </summary>
public sealed record CallNextInQueueCommand(string BranchId) : ICommand<string?>;
