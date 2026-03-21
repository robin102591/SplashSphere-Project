using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Transactions.Commands.UpdateDiscountTip;

public sealed class UpdateDiscountTipCommandHandler(
    IApplicationDbContext context)
    : IRequestHandler<UpdateDiscountTipCommand, Result>
{
    public async Task<Result> Handle(
        UpdateDiscountTipCommand request,
        CancellationToken cancellationToken)
    {
        var transaction = await context.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
            return Result.Failure(Error.NotFound("Transaction", request.TransactionId));

        if (transaction.Status is TransactionStatus.Completed
                               or TransactionStatus.Cancelled
                               or TransactionStatus.Refunded)
        {
            return Result.Failure(Error.Validation(
                $"Cannot update discount/tip on a transaction with status '{transaction.Status}'."));
        }

        // Discount cannot exceed the subtotal
        if (request.DiscountAmount > transaction.TotalAmount)
        {
            return Result.Failure(Error.Validation(
                $"Discount (₱{request.DiscountAmount:N2}) cannot exceed subtotal (₱{transaction.TotalAmount:N2})."));
        }

        var newFinalAmount = transaction.TotalAmount - request.DiscountAmount + transaction.TaxAmount;

        // Existing payments must not exceed the new customer total
        var alreadyPaid = await context.Payments
            .Where(p => p.TransactionId == request.TransactionId)
            .SumAsync(p => p.Amount, cancellationToken);

        var newCustomerOwes = newFinalAmount + request.TipAmount;

        if (alreadyPaid > newCustomerOwes + 0.01m)
        {
            return Result.Failure(Error.Validation(
                $"Payments already made (₱{alreadyPaid:N2}) exceed the new total (₱{newCustomerOwes:N2}). " +
                "Reduce discount or increase tip."));
        }

        transaction.DiscountAmount = request.DiscountAmount;
        transaction.TipAmount      = request.TipAmount;
        transaction.FinalAmount    = newFinalAmount;

        return Result.Success();
    }
}
