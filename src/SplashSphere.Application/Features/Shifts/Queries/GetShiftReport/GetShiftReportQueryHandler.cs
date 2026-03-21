using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Application.Features.Shifts.Queries.GetCurrentShift;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Shifts.Queries.GetShiftReport;

public sealed class GetShiftReportQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetShiftReportQuery, ShiftReportDto?>
{
    public async Task<ShiftReportDto?> Handle(
        GetShiftReportQuery request,
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

        if (shift is null) return null;

        var closedAt = shift.ClosedAt ?? DateTime.UtcNow;

        // Top services — join TransactionService → Service for names
        // UnitPrice is the resolved price per TransactionService
        var topServices = await db.TransactionServices
            .Where(ts =>
                ts.Transaction.BranchId == shift.BranchId &&
                ts.Transaction.CashierId == shift.CashierId &&
                ts.Transaction.Status == TransactionStatus.Completed &&
                ts.Transaction.CompletedAt >= shift.OpenedAt &&
                ts.Transaction.CompletedAt <= closedAt)
            .GroupBy(ts => new { ts.ServiceId, ts.Service.Name })
            .Select(g => new TopServiceDto(
                g.Key.Name,
                g.Count(),
                g.Sum(ts => ts.UnitPrice)))
            .OrderByDescending(s => s.TotalAmount)
            .Take(10)
            .ToListAsync(cancellationToken);

        // Also include packages in top services
        var topPackages = await db.TransactionPackages
            .Where(tp =>
                tp.Transaction.BranchId == shift.BranchId &&
                tp.Transaction.CashierId == shift.CashierId &&
                tp.Transaction.Status == TransactionStatus.Completed &&
                tp.Transaction.CompletedAt >= shift.OpenedAt &&
                tp.Transaction.CompletedAt <= closedAt)
            .GroupBy(tp => new { tp.PackageId, tp.Package.Name })
            .Select(g => new TopServiceDto(
                g.Key.Name,
                g.Count(),
                g.Sum(tp => tp.UnitPrice)))
            .OrderByDescending(p => p.TotalAmount)
            .Take(5)
            .ToListAsync(cancellationToken);

        var combinedTopServices = topServices
            .Concat(topPackages)
            .OrderByDescending(s => s.TotalAmount)
            .Take(10)
            .ToList();

        // Top employees by commission
        // TransactionEmployee.Employee is Employee entity (has FullName)
        var topEmployees = await db.TransactionEmployees
            .Where(te =>
                te.Transaction.BranchId == shift.BranchId &&
                te.Transaction.CashierId == shift.CashierId &&
                te.Transaction.Status == TransactionStatus.Completed &&
                te.Transaction.CompletedAt >= shift.OpenedAt &&
                te.Transaction.CompletedAt <= closedAt)
            .GroupBy(te => new { te.EmployeeId, te.Employee.FirstName, te.Employee.LastName })
            .Select(g => new TopEmployeeDto(
                g.Key.EmployeeId,
                g.Key.FirstName + " " + g.Key.LastName,
                g.Count(),
                g.Sum(te => te.TotalCommission)))
            .OrderByDescending(e => e.TotalCommission)
            .Take(10)
            .ToListAsync(cancellationToken);

        return new ShiftReportDto(
            GetCurrentShiftQueryHandler.MapToDetail(shift),
            combinedTopServices,
            topEmployees,
            DateTime.UtcNow);
    }
}
