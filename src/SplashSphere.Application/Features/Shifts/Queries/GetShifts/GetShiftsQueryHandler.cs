using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Shifts.Queries.GetShifts;

public sealed class GetShiftsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetShiftsQuery, PagedResult<ShiftSummaryDto>>
{
    public async Task<PagedResult<ShiftSummaryDto>> Handle(
        GetShiftsQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.CashierShifts
            .Include(s => s.Branch)
            .Include(s => s.Cashier)
            .Include(s => s.ReviewedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            query = query.Where(s => s.BranchId == request.BranchId);

        if (!string.IsNullOrWhiteSpace(request.CashierId))
            query = query.Where(s => s.CashierId == request.CashierId);

        if (request.DateFrom.HasValue)
            query = query.Where(s => s.ShiftDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(s => s.ShiftDate <= request.DateTo.Value);

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        if (request.ReviewStatus.HasValue)
            query = query.Where(s => s.ReviewStatus == request.ReviewStatus.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.OpenedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new ShiftSummaryDto(
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
                s.TotalRevenue,
                s.Variance,
                s.ReviewStatus,
                s.ReviewedBy != null ? s.ReviewedBy.FullName : null,
                s.ReviewedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<ShiftSummaryDto>.Create(items, total, request.Page, request.PageSize);
    }
}
