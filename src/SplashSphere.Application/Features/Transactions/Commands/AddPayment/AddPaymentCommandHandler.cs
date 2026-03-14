using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Transactions.Commands.AddPayment;

public sealed class AddPaymentCommandHandler(
    IApplicationDbContext context,
    IEventPublisher eventPublisher)
    : IRequestHandler<AddPaymentCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        AddPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var transaction = await context.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
            return Result.Failure<string>(Error.NotFound("Transaction", request.TransactionId));

        // Payments can only be added to active (non-terminal) transactions
        if (transaction.Status is TransactionStatus.Completed
                               or TransactionStatus.Cancelled
                               or TransactionStatus.Refunded)
        {
            return Result.Failure<string>(Error.Validation(
                $"Cannot add a payment to a transaction with status '{transaction.Status}'."));
        }

        // Guard against over-payment
        var alreadyPaid = await context.Payments
            .Where(p => p.TransactionId == request.TransactionId)
            .SumAsync(p => p.Amount, cancellationToken);

        var remaining = transaction.FinalAmount - alreadyPaid;

        if (request.Amount > remaining + 0.01m) // 1-cent tolerance for floating-point drift
        {
            return Result.Failure<string>(Error.Validation(
                $"Payment of ₱{request.Amount:N2} exceeds remaining balance of ₱{remaining:N2}."));
        }

        var payment = new Payment(
            transaction.TenantId,
            request.TransactionId,
            request.PaymentMethod,
            request.Amount,
            request.ReferenceNumber);

        context.Payments.Add(payment);

        // ── Auto-complete when fully paid ─────────────────────────────────────
        var newTotal = alreadyPaid + request.Amount;
        var isFullyPaid = newTotal >= transaction.FinalAmount - 0.01m;

        if (isFullyPaid && transaction.Status != TransactionStatus.Completed)
        {
            var previousStatus = transaction.Status;
            transaction.Status      = TransactionStatus.Completed;
            transaction.CompletedAt = DateTime.UtcNow;

            // Propagate completion to the linked queue entry
            var queueEntry = await context.QueueEntries
                .FirstOrDefaultAsync(q => q.TransactionId == request.TransactionId, cancellationToken);

            if (queueEntry is not null && queueEntry.Status == QueueStatus.InService)
            {
                queueEntry.Status      = QueueStatus.Completed;
                queueEntry.CompletedAt = DateTime.UtcNow;
            }

            await eventPublisher.PublishAsync(new TransactionStatusChangedEvent(
                transaction.Id,
                transaction.TenantId,
                transaction.BranchId,
                previousStatus,
                TransactionStatus.Completed,
                queueEntry?.Id), cancellationToken);

            await eventPublisher.PublishAsync(new TransactionCompletedEvent(
                transaction.Id,
                transaction.TenantId,
                transaction.BranchId,
                transaction.TransactionNumber,
                transaction.FinalAmount,
                queueEntry?.Id), cancellationToken);
        }

        return Result.Success(payment.Id);
    }
}
