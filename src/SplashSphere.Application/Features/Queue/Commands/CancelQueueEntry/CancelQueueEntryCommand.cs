using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Commands.CancelQueueEntry;

/// <summary>
/// Cancels a queue entry. Allowed from Waiting or Called status only.
/// Once InService, cancellation must go through the transaction (cancel the transaction).
/// </summary>
public sealed record CancelQueueEntryCommand(string QueueEntryId, string? Reason = null) : ICommand;
