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

        // Project to scalars first — EF Core cannot translate GroupBy when navigation
        // properties appear in the key selector. Fetch flat rows then group in memory.

        var serviceRows = await db.TransactionServices
            .Where(ts =>
                ts.Transaction.BranchId == shift.BranchId &&
                ts.Transaction.CashierId == shift.CashierId &&
                ts.Transaction.Status == TransactionStatus.Completed &&
                ts.Transaction.CompletedAt >= shift.OpenedAt &&
                ts.Transaction.CompletedAt <= closedAt)
            .Select(ts => new { ts.ServiceId, ServiceName = ts.Service.Name, ts.UnitPrice })
            .ToListAsync(cancellationToken);

        var topServices = serviceRows
            .GroupBy(r => new { r.ServiceId, r.ServiceName })
            .Select(g => new TopServiceDto(g.Key.ServiceName, g.Count(), g.Sum(r => r.UnitPrice)))
            .OrderByDescending(s => s.TotalAmount)
            .Take(10)
            .ToList();

        var packageRows = await db.TransactionPackages
            .Where(tp =>
                tp.Transaction.BranchId == shift.BranchId &&
                tp.Transaction.CashierId == shift.CashierId &&
                tp.Transaction.Status == TransactionStatus.Completed &&
                tp.Transaction.CompletedAt >= shift.OpenedAt &&
                tp.Transaction.CompletedAt <= closedAt)
            .Select(tp => new { tp.PackageId, PackageName = tp.Package.Name, tp.UnitPrice })
            .ToListAsync(cancellationToken);

        var topPackages = packageRows
            .GroupBy(r => new { r.PackageId, r.PackageName })
            .Select(g => new TopServiceDto(g.Key.PackageName, g.Count(), g.Sum(r => r.UnitPrice)))
            .OrderByDescending(p => p.TotalAmount)
            .Take(5)
            .ToList();

        var combinedTopServices = topServices
            .Concat(topPackages)
            .OrderByDescending(s => s.TotalAmount)
            .Take(10)
            .ToList();

        var employeeRows = await db.TransactionEmployees
            .Where(te =>
                te.Transaction.BranchId == shift.BranchId &&
                te.Transaction.CashierId == shift.CashierId &&
                te.Transaction.Status == TransactionStatus.Completed &&
                te.Transaction.CompletedAt >= shift.OpenedAt &&
                te.Transaction.CompletedAt <= closedAt)
            .Select(te => new
            {
                te.EmployeeId,
                te.Employee.FirstName,
                te.Employee.LastName,
                te.TotalCommission
            })
            .ToListAsync(cancellationToken);

        var topEmployees = employeeRows
            .GroupBy(r => new { r.EmployeeId, r.FirstName, r.LastName })
            .Select(g => new TopEmployeeDto(
                g.Key.EmployeeId,
                g.Key.FirstName + " " + g.Key.LastName,
                g.Count(),
                g.Sum(r => r.TotalCommission)))
            .OrderByDescending(e => e.TotalCommission)
            .Take(10)
            .ToList();

        return new ShiftReportDto(
            GetCurrentShiftQueryHandler.MapToDetail(shift),
            combinedTopServices,
            topEmployees,
            DateTime.UtcNow);
    }
}
