using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Shifts.Queries.GetCurrentShift;

namespace SplashSphere.Application.Features.Shifts.Queries.GetShiftById;

public sealed class GetShiftByIdQueryHandler(
    IApplicationDbContext db)
    : IRequestHandler<GetShiftByIdQuery, ShiftDetailDto?>
{
    public async Task<ShiftDetailDto?> Handle(
        GetShiftByIdQuery request,
        CancellationToken cancellationToken)
    {
        var shift = await db.CashierShifts
            .Include(s => s.Branch)
            .Include(s => s.Cashier)
            .Include(s => s.ReviewedBy)
            .Include(s => s.CashMovements)
            .Include(s => s.Denominations)
            .Include(s => s.PaymentSummaries)
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId, cancellationToken);

        return shift is null ? null : GetCurrentShiftQueryHandler.MapToDetail(shift);
    }
}
