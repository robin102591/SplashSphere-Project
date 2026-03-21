using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Shifts.Queries.GetShiftVarianceReport;

public sealed class GetShiftVarianceReportQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetShiftVarianceReportQuery, ShiftVarianceReportDto>
{
    public async Task<ShiftVarianceReportDto> Handle(
        GetShiftVarianceReportQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.CashierShifts
            .Include(s => s.Cashier)
            .Where(s => s.Status == ShiftStatus.Closed)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            query = query.Where(s => s.BranchId == request.BranchId);

        if (!string.IsNullOrWhiteSpace(request.CashierId))
            query = query.Where(s => s.CashierId == request.CashierId);

        if (request.DateFrom.HasValue)
            query = query.Where(s => s.ShiftDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(s => s.ShiftDate <= request.DateTo.Value);

        var shifts = await query.ToListAsync(cancellationToken);

        var cashierSummaries = shifts
            .GroupBy(s => new { s.CashierId, s.Cashier.FullName })
            .Select(g => new ShiftVarianceCashierDto(
                g.Key.CashierId,
                g.Key.FullName,
                g.Count(),
                g.Sum(s => s.Variance),
                g.Count() > 0 ? g.Sum(s => s.Variance) / g.Count() : 0,
                g.Where(s => s.Variance < 0).Select(s => s.Variance).DefaultIfEmpty(0).Min()))
            .OrderBy(c => c.TotalVariance)
            .ToList();

        IReadOnlyList<VarianceTrendPointDto>? trendPoints = null;
        if (!string.IsNullOrWhiteSpace(request.CashierId))
        {
            trendPoints = shifts
                .OrderBy(s => s.ShiftDate)
                .Select(s => new VarianceTrendPointDto(s.ShiftDate, s.Variance, s.ReviewStatus))
                .ToList();
        }

        return new ShiftVarianceReportDto(cashierSummaries, trendPoints);
    }
}
