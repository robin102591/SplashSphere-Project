using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Transactions.Commands.UpdateTransactionStatus;

public sealed class UpdateTransactionStatusCommandHandler(
    IApplicationDbContext context,
    IEventPublisher eventPublisher)
    : IRequestHandler<UpdateTransactionStatusCommand, Result>
{
    public async Task<Result> Handle(
        UpdateTransactionStatusCommand request,
        CancellationToken cancellationToken)
    {
        var transaction = await context.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
            return Result.Failure(Error.NotFound("Transaction", request.TransactionId));

        // ── Validate the transition ───────────────────────────────────────────
        var validationError = ValidateTransition(transaction.Status, request.NewStatus);
        if (validationError is not null)
            return Result.Failure(validationError);

        var previousStatus = transaction.Status;
        transaction.Status = request.NewStatus;

        switch (request.NewStatus)
        {
            case TransactionStatus.Completed:
                transaction.CompletedAt = DateTime.UtcNow;
                break;

            case TransactionStatus.Cancelled:
                transaction.CancelledAt = DateTime.UtcNow;
                break;
        }

        // ── Propagate to linked queue entry ───────────────────────────────────
        var queueEntry = await context.QueueEntries
            .FirstOrDefaultAsync(q => q.TransactionId == request.TransactionId, cancellationToken);

        if (queueEntry is not null)
        {
            switch (request.NewStatus)
            {
                case TransactionStatus.Completed:
                    queueEntry.Status      = QueueStatus.Completed;
                    queueEntry.CompletedAt = DateTime.UtcNow;
                    break;

                case TransactionStatus.Cancelled:
                    // Only update the queue entry if it is still in the InService state.
                    // If someone manually cancelled an already-completed queue entry, leave it alone.
                    if (queueEntry.Status == QueueStatus.InService)
                    {
                        queueEntry.Status      = QueueStatus.Cancelled;
                        queueEntry.CancelledAt = DateTime.UtcNow;
                    }
                    break;
            }
        }

        // ── Publish events ────────────────────────────────────────────────────
        await eventPublisher.PublishAsync(new TransactionStatusChangedEvent(
            transaction.Id,
            transaction.TenantId,
            transaction.BranchId,
            previousStatus,
            request.NewStatus,
            queueEntry?.Id), cancellationToken);

        if (request.NewStatus == TransactionStatus.Completed)
        {
            await eventPublisher.PublishAsync(new TransactionCompletedEvent(
                transaction.Id,
                transaction.TenantId,
                transaction.BranchId,
                transaction.TransactionNumber,
                transaction.FinalAmount,
                queueEntry?.Id), cancellationToken);
        }

        return Result.Success();
    }

    // ── Valid transition table ────────────────────────────────────────────────
    //  Pending     → InProgress, Cancelled
    //  InProgress  → Completed, Cancelled
    //  Completed   → Refunded
    //  Cancelled   → (terminal)
    //  Refunded    → (terminal)
    private static Error? ValidateTransition(TransactionStatus current, TransactionStatus next)
    {
        var allowed = current switch
        {
            TransactionStatus.Pending    => new[] { TransactionStatus.InProgress, TransactionStatus.Cancelled },
            TransactionStatus.InProgress => new[] { TransactionStatus.Completed,  TransactionStatus.Cancelled },
            TransactionStatus.Completed  => new[] { TransactionStatus.Refunded },
            _                            => Array.Empty<TransactionStatus>(),
        };

        if (Array.IndexOf(allowed, next) < 0)
            return Error.Validation(
                $"Cannot transition from '{current}' to '{next}'. " +
                $"Allowed transitions: {(allowed.Length > 0 ? string.Join(", ", allowed.Select(s => s.ToString())) : "none (terminal state)")}.");

        return null;
    }
}
