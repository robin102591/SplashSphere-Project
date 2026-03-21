using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Shifts.Queries.GetCurrentShift;

public sealed class GetCurrentShiftQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetCurrentShiftQuery, ShiftDetailDto?>
{
    public async Task<ShiftDetailDto?> Handle(
        GetCurrentShiftQuery request,
        CancellationToken cancellationToken)
    {
        var shift = await db.CashierShifts
            .Include(s => s.Branch)
            .Include(s => s.Cashier)
            .Include(s => s.ReviewedBy)
            .Include(s => s.CashMovements)
            .Include(s => s.Denominations)
            .Include(s => s.PaymentSummaries)
            .FirstOrDefaultAsync(s =>
                s.CashierId == tenantContext.UserId &&
                s.BranchId == request.BranchId &&
                s.Status == ShiftStatus.Open,
                cancellationToken);

        return shift is null ? null : MapToDetail(shift);
    }

    internal static ShiftDetailDto MapToDetail(Domain.Entities.CashierShift s) => new(
        s.Id,
        s.BranchId,
        s.Branch.Name,
        s.CashierId,
        s.Cashier.FullName,
        s.ShiftDate,
        s.OpenedAt,
        s.ClosedAt,
        s.Status,
        s.OpeningCashFund,
        s.TotalCashPayments,
        s.TotalNonCashPayments,
        s.TotalCashIn,
        s.TotalCashOut,
        s.ExpectedCashInDrawer,
        s.ActualCashInDrawer,
        s.Variance,
        s.TotalTransactionCount,
        s.TotalRevenue,
        s.TotalCommissions,
        s.TotalDiscounts,
        s.ReviewStatus,
        s.ReviewedById,
        s.ReviewedBy?.FullName,
        s.ReviewedAt,
        s.ReviewNotes,
        s.CashMovements
            .OrderBy(m => m.MovementTime)
            .Select(m => new CashMovementDto(m.Id, m.Type, m.Amount, m.Reason, m.Reference, m.MovementTime))
            .ToList(),
        s.Denominations
            .OrderByDescending(d => d.DenominationValue)
            .Select(d => new ShiftDenominationDto(d.DenominationValue, d.Count, d.Subtotal))
            .ToList(),
        s.PaymentSummaries
            .OrderBy(p => p.Method)
            .Select(p => new ShiftPaymentSummaryDto(p.Method, p.TransactionCount, p.TotalAmount))
            .ToList());
}
