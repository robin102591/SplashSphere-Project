using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.CloseShift;

public sealed class CloseShiftCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<CloseShiftCommand, Result>
{
    public async Task<Result> Handle(
        CloseShiftCommand request,
        CancellationToken cancellationToken)
    {
        var shift = await db.CashierShifts
            .Include(s => s.CashMovements)
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift is null)
            return Result.Failure(Error.NotFound("Shift not found."));

        if (shift.Status != ShiftStatus.Open)
            return Result.Failure(Error.Conflict("Only an open shift can be closed."));

        if (shift.CashierId != tenantContext.UserId &&
            tenantContext.Role is not ("org:admin" or "org:manager"))
            return Result.Failure(Error.Forbidden("You are not allowed to close this shift."));

        var nowUtc = DateTime.UtcNow;

        // ── Step 1: Query completed transactions during this shift ───────────────
        // Query Transactions directly first — navigating p.Transaction.* in a Where
        // clause causes EF Core to apply the Transaction global query filter on the
        // JOIN, which can produce 0 rows. Fetching IDs first is always safe.
        var transactionIds = await db.Transactions
            .Where(t =>
                t.BranchId == shift.BranchId &&
                t.CashierId == shift.CashierId &&
                t.Status == TransactionStatus.Completed &&
                t.CompletedAt >= shift.OpenedAt &&
                t.CompletedAt <= nowUtc)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var payments = transactionIds.Count == 0
            ? []
            : await db.Payments
                .Where(p => transactionIds.Contains(p.TransactionId))
                .Select(p => new { p.PaymentMethod, p.Amount, p.TransactionId })
                .ToListAsync(cancellationToken);

        var commissions = await db.TransactionEmployees
            .Where(te => transactionIds.Contains(te.TransactionId))
            .SumAsync(te => te.TotalCommission, cancellationToken);

        var discounts = await db.Transactions
            .Where(t => transactionIds.Contains(t.Id))
            .SumAsync(t => t.DiscountAmount, cancellationToken);

        // ── Step 2: Group payments by method ────────────────────────────────────
        var paymentGroups = payments
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new ShiftPaymentSummary(
                shift.TenantId,
                shift.Id,
                g.Key,
                g.Select(p => p.TransactionId).Distinct().Count(),
                g.Sum(p => p.Amount)))
            .ToList();

        foreach (var summary in paymentGroups)
            db.ShiftPaymentSummaries.Add(summary);

        // ── Step 3: Compute totals ───────────────────────────────────────────────
        shift.TotalCashPayments = paymentGroups
            .Where(s => s.Method == PaymentMethod.Cash)
            .Sum(s => s.TotalAmount);

        shift.TotalNonCashPayments = paymentGroups
            .Where(s => s.Method != PaymentMethod.Cash)
            .Sum(s => s.TotalAmount);

        shift.TotalRevenue = shift.TotalCashPayments + shift.TotalNonCashPayments;
        shift.TotalTransactionCount = transactionIds.Count;
        shift.TotalCommissions = commissions;
        shift.TotalDiscounts = discounts;

        shift.TotalCashIn  = shift.CashMovements
            .Where(m => m.Type == CashMovementType.CashIn)
            .Sum(m => m.Amount);

        shift.TotalCashOut = shift.CashMovements
            .Where(m => m.Type == CashMovementType.CashOut)
            .Sum(m => m.Amount);

        shift.ExpectedCashInDrawer =
            shift.OpeningCashFund +
            shift.TotalCashPayments +
            shift.TotalCashIn -
            shift.TotalCashOut;

        // ── Step 4: Denomination count ───────────────────────────────────────────
        foreach (var entry in request.Denominations.Where(d => d.Count > 0))
        {
            db.ShiftDenominations.Add(new ShiftDenomination(
                shift.TenantId,
                shift.Id,
                entry.DenominationValue,
                entry.Count));
        }

        shift.ActualCashInDrawer = request.Denominations
            .Sum(d => d.DenominationValue * d.Count);

        shift.Variance = shift.ActualCashInDrawer - shift.ExpectedCashInDrawer;

        // ── Step 5: Auto-review based on tenant thresholds ──────────────────────
        var settings = await db.ShiftSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantContext.TenantId, cancellationToken);

        var autoApprove  = settings?.AutoApproveThreshold ?? 50m;
        var flagThreshold = settings?.FlagThreshold ?? 200m;

        var absVariance = Math.Abs(shift.Variance);
        shift.ReviewStatus = absVariance <= autoApprove
            ? ReviewStatus.Approved
            : absVariance > flagThreshold
                ? ReviewStatus.Flagged
                : ReviewStatus.Pending;

        shift.Status   = ShiftStatus.Closed;
        shift.ClosedAt = nowUtc;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
