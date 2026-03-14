using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Commands.MarkNoShow;

/// <summary>
/// Marks a queue entry as NoShow. Dispatched by the Hangfire 5-minute timer
/// scheduled in <see cref="Commands.CallNextInQueue.CallNextInQueueCommandHandler"/>.
/// <para>
/// <b>Idempotent:</b> if the entry is no longer in <see cref="Domain.Enums.QueueStatus.Called"/>
/// state when this fires (e.g. cashier already started service), the command
/// succeeds silently without changing anything.
/// </para>
/// After marking NoShow, automatically calls the next WAITING entry in the branch.
/// </summary>
public sealed record MarkNoShowCommand(string QueueEntryId) : ICommand;
