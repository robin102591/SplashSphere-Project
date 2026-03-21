using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.RecordCashMovement;

public sealed class RecordCashMovementCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<RecordCashMovementCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        RecordCashMovementCommand request,
        CancellationToken cancellationToken)
    {
        var shift = await db.CashierShifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift is null)
            return Result.Failure<string>(Error.NotFound("Shift not found."));

        if (shift.Status != ShiftStatus.Open)
            return Result.Failure<string>(
                Error.Conflict("Cash movements can only be recorded on an open shift."));

        // Only the owning cashier or a manager may record movements
        if (shift.CashierId != tenantContext.UserId &&
            tenantContext.Role is not ("org:admin" or "org:manager"))
            return Result.Failure<string>(
                Error.Forbidden("You are not allowed to record movements for this shift."));

        var movement = new CashMovement(
            shift.TenantId,
            shift.Id,
            request.Type,
            request.Amount,
            request.Reason,
            request.Reference,
            DateTime.UtcNow);

        db.CashMovements.Add(movement);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success<string>(movement.Id);
    }
}
