using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.VoidShift;

public sealed class VoidShiftCommandHandler(
    IApplicationDbContext db)
    : IRequestHandler<VoidShiftCommand, Result>
{
    public async Task<Result> Handle(
        VoidShiftCommand request,
        CancellationToken cancellationToken)
    {
        var shift = await db.CashierShifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift is null)
            return Result.Failure(Error.NotFound("Shift not found."));

        if (shift.Status == ShiftStatus.Voided)
            return Result.Failure(Error.Conflict("Shift is already voided."));

        // Check no transactions were processed during this shift
        var hasTransactions = await db.Transactions
            .AnyAsync(t =>
                t.BranchId == shift.BranchId &&
                t.CashierId == shift.CashierId &&
                t.Status == TransactionStatus.Completed &&
                t.CompletedAt >= shift.OpenedAt,
                cancellationToken);

        if (hasTransactions)
            return Result.Failure(
                Error.Conflict("Cannot void a shift that has completed transactions. Close it instead."));

        shift.Status      = ShiftStatus.Voided;
        shift.ClosedAt    = DateTime.UtcNow;
        shift.ReviewNotes = request.Reason;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
