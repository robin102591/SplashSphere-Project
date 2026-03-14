using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Transactions.Commands.UpdateTransactionStatus;

/// <summary>
/// Transitions a transaction to a new status, enforcing the state machine:
/// Pending → InProgress → Completed; Pending/InProgress → Cancelled; Completed → Refunded.
/// When the transition affects the linked queue entry, the queue entry is updated accordingly:
/// Completed → QueueEntry Completed; Cancelled → QueueEntry Cancelled (if InService).
/// </summary>
public sealed record UpdateTransactionStatusCommand(
    string TransactionId,
    TransactionStatus NewStatus) : ICommand;
