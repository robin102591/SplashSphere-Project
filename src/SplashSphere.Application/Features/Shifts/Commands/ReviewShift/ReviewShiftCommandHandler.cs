using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.ReviewShift;

public sealed class ReviewShiftCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<ReviewShiftCommand, Result>
{
    public async Task<Result> Handle(
        ReviewShiftCommand request,
        CancellationToken cancellationToken)
    {
        var shift = await db.CashierShifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        if (shift is null)
            return Result.Failure(Error.NotFound("Shift not found."));

        if (shift.Status != ShiftStatus.Closed)
            return Result.Failure(Error.Conflict("Only closed shifts can be reviewed."));

        shift.ReviewStatus = request.NewReviewStatus;
        shift.ReviewedById = tenantContext.UserId;
        shift.ReviewedAt   = DateTime.UtcNow;
        shift.ReviewNotes  = request.Notes;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
