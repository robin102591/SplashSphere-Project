using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Transactions.Commands.RefundTransaction;

public sealed class RefundTransactionCommandHandler(
    IApplicationDbContext context,
    IEventPublisher eventPublisher)
    : IRequestHandler<RefundTransactionCommand, Result>
{
    // PHT offset — used to map the UTC CompletedAt to the local date for payroll period lookup
    private static readonly TimeSpan Pht = TimeSpan.FromHours(8);

    public async Task<Result> Handle(
        RefundTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // ── Step 1: Load transaction with commission + merchandise detail ─────

        var transaction = await context.Transactions
            .Include(t => t.Employees)
            .Include(t => t.Merchandise)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        if (transaction is null)
            return Result.Failure(Error.NotFound("Transaction", request.TransactionId));

        if (transaction.Status != TransactionStatus.Completed)
            return Result.Failure(Error.Validation(
                $"Only Completed transactions can be refunded. Current status: {transaction.Status}."));

        // ── Step 2: Locate the payroll period that covers the completion date ─

        // CompletedAt is UTC — convert to PHT date for period boundary comparison
        var completedDatePht = DateOnly.FromDateTime(transaction.CompletedAt!.Value.Add(Pht));

        var period = await context.PayrollPeriods
            .FirstOrDefaultAsync(
                p => p.TenantId == transaction.TenantId
                     && p.StartDate <= completedDatePht
                     && p.EndDate   >= completedDatePht,
                cancellationToken);

        // ── Step 3: Guard — cannot refund if the period is already processed ─

        if (period?.Status == PayrollStatus.Processed)
            return Result.Failure(Error.Validation(
                "Cannot refund this transaction — the payroll period covering its completion date " +
                "has already been processed. Please handle the commission adjustment manually."));

        // ── Step 4: Claw back commissions from PayrollEntries (Open or Closed period) ─

        if (period is not null && transaction.Employees.Count > 0)
        {
            var employeeIds = transaction.Employees.Select(e => e.EmployeeId).ToList();

            var payrollEntries = await context.PayrollEntries
                .Where(e => e.PayrollPeriodId == period.Id && employeeIds.Contains(e.EmployeeId))
                .ToListAsync(cancellationToken);

            foreach (var txEmployee in transaction.Employees)
            {
                var entry = payrollEntries.FirstOrDefault(e => e.EmployeeId == txEmployee.EmployeeId);
                if (entry is null) continue;

                // Subtract the commission earned in this transaction; floor at zero
                entry.TotalCommissions = Math.Max(0m,
                    entry.TotalCommissions - txEmployee.TotalCommission);
            }
        }

        // ── Step 5: Restore merchandise stock ────────────────────────────────

        if (transaction.Merchandise.Count > 0)
        {
            var merchandiseIds = transaction.Merchandise.Select(m => m.MerchandiseId).ToList();

            var merchandiseItems = await context.Merchandise
                .Where(m => merchandiseIds.Contains(m.Id))
                .ToListAsync(cancellationToken);

            foreach (var txMerch in transaction.Merchandise)
            {
                var item = merchandiseItems.FirstOrDefault(m => m.Id == txMerch.MerchandiseId);
                if (item is null) continue;

                item.StockQuantity += txMerch.Quantity;
            }
        }

        // ── Step 6: Transition transaction status ────────────────────────────

        transaction.Status       = TransactionStatus.Refunded;
        transaction.RefundedAt   = DateTime.UtcNow;
        transaction.RefundReason = request.Reason;

        // ── Step 7: Publish event ─────────────────────────────────────────────

        eventPublisher.Enqueue(new TransactionRefundedEvent(
            transaction.Id,
            transaction.TenantId,
            transaction.BranchId,
            transaction.TransactionNumber,
            transaction.FinalAmount));

        return Result.Success();
    }
}
