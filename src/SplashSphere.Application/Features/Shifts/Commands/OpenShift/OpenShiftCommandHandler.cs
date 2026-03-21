using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Commands.OpenShift;

public sealed class OpenShiftCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<OpenShiftCommand, Result<string>>
{
    private static readonly TimeZoneInfo Manila =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

    public async Task<Result<string>> Handle(
        OpenShiftCommand request,
        CancellationToken cancellationToken)
    {
        // Validate branch belongs to tenant
        var branch = await db.Branches
            .FirstOrDefaultAsync(b => b.Id == request.BranchId, cancellationToken);

        if (branch is null)
            return Result.Failure<string>(Error.NotFound("Branch not found."));

        // Check no open shift exists for this cashier at this branch
        var hasOpenShift = await db.CashierShifts
            .AnyAsync(s =>
                s.CashierId == tenantContext.UserId &&
                s.BranchId == request.BranchId &&
                s.Status == ShiftStatus.Open,
                cancellationToken);

        if (hasOpenShift)
            return Result.Failure<string>(
                Error.Conflict("You already have an open shift at this branch. Close it before opening a new one."));

        // Determine business date (Manila local; before 06:00 → previous day)
        var nowUtc = DateTime.UtcNow;
        var nowManila = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, Manila);
        var shiftDate = nowManila.Hour < 6
            ? DateOnly.FromDateTime(nowManila.AddDays(-1))
            : DateOnly.FromDateTime(nowManila);

        var shift = new CashierShift(
            tenantContext.TenantId,
            request.BranchId,
            tenantContext.UserId,
            shiftDate,
            nowUtc,
            request.OpeningCashFund);

        db.CashierShifts.Add(shift);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success<string>(shift.Id);
    }
}
