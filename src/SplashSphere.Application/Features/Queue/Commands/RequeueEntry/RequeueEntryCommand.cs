using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Commands.RequeueEntry;

/// <summary>
/// Re-queues a <see cref="QueueStatus.NoShow"/> entry back to <see cref="QueueStatus.Waiting"/>.
/// Optionally allows upgrading the priority (e.g. cashier bumps a returning no-show to Express).
/// Recalculates the estimated wait time based on current WAITING count.
/// </summary>
public sealed record RequeueEntryCommand(
    string QueueEntryId,
    QueuePriority? NewPriority = null) : ICommand;
