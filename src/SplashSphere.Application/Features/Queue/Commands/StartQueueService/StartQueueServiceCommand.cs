using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Queue.Commands.StartQueueService;

/// <summary>
/// Links an already-created transaction to a queue entry and advances
/// the entry from <see cref="Domain.Enums.QueueStatus.Called"/> to
/// <see cref="Domain.Enums.QueueStatus.InService"/>.
/// <para>
/// POS flow: cashier opens the New Transaction screen from the queue board
/// (URL includes <c>?queueEntryId=xxx</c> to pre-fill vehicle/services),
/// submits the transaction via <c>POST /transactions</c>, then calls this
/// endpoint with the resulting TransactionId to link them.
/// </para>
/// The no-show timer is rendered inert automatically — <see cref="Commands.MarkNoShow.MarkNoShowCommand"/>
/// checks the entry's status before acting, so transitioning to InService here
/// is sufficient; no explicit Hangfire job cancellation is required.
/// </summary>
public sealed record StartQueueServiceCommand(
    string QueueEntryId,
    string TransactionId) : ICommand;
