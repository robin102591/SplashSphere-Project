using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.ReopenShift;

public sealed class ReopenShiftCommandHandler(
    IApplicationDbContext db)
    : IRequestHandler<ReopenShiftCommand, Result>
{
    public async Task<Result> Handle(
        ReopenShiftCommand request,
        CancellationToken cancellationToken)
    {
        var shift = await db.CashierShifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift is null)
            return Result.Failure(Error.NotFound("Shift not found."));

        if (shift.Status != ShiftStatus.Closed)
            return Result.Failure(Error.Conflict("Only closed shifts can be reopened."));

        if (shift.ReviewStatus is ReviewStatus.Approved or ReviewStatus.Flagged)
            return Result.Failure(
                Error.Conflict("Shifts that have been approved or flagged cannot be reopened."));

        // Remove stale denomination and payment summary records so they can be re-submitted
        var denominations = await db.ShiftDenominations
            .Where(d => d.CashierShiftId == shift.Id)
            .ToListAsync(cancellationToken);
        db.ShiftDenominations.RemoveRange(denominations);

        var summaries = await db.ShiftPaymentSummaries
            .Where(s => s.CashierShiftId == shift.Id)
            .ToListAsync(cancellationToken);
        db.ShiftPaymentSummaries.RemoveRange(summaries);

        // Reset computed totals
        shift.Status                = ShiftStatus.Open;
        shift.ClosedAt              = null;
        shift.TotalCashPayments     = 0;
        shift.TotalNonCashPayments  = 0;
        shift.TotalCashIn           = 0;
        shift.TotalCashOut          = 0;
        shift.ExpectedCashInDrawer  = 0;
        shift.ActualCashInDrawer    = 0;
        shift.Variance              = 0;
        shift.TotalTransactionCount = 0;
        shift.TotalRevenue          = 0;
        shift.TotalCommissions      = 0;
        shift.TotalDiscounts        = 0;
        shift.ReviewStatus          = ReviewStatus.Pending;
        shift.ReviewedById          = null;
        shift.ReviewedAt            = null;
        shift.ReviewNotes           = null;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
