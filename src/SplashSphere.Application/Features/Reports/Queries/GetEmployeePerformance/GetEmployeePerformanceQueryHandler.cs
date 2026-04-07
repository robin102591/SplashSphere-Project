using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Reports.Queries.GetEmployeePerformance;

public sealed class GetEmployeePerformanceQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetEmployeePerformanceQuery, EmployeePerformanceDto>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<EmployeePerformanceDto> Handle(
        GetEmployeePerformanceQuery request,
        CancellationToken cancellationToken)
    {
        var fromUtc = DateTime.SpecifyKind(request.From.ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(request.To.AddDays(1).ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);

        // ── Active employees ────────────────────────────────────────────────
        var empQuery = context.Employees.AsNoTracking().Where(e => e.IsActive);
        if (request.BranchId is not null)
            empQuery = empQuery.Where(e => e.BranchId == request.BranchId);

        var employees = await empQuery
            .Select(e => new
            {
                e.Id,
                Name = (e.FirstName + " " + e.LastName).Trim(),
                BranchName = e.Branch.Name,
                e.EmployeeType,
            })
            .ToListAsync(cancellationToken);

        var employeeIds = employees.Select(e => e.Id).ToList();

        // ── Service assignments (from completed transactions) ───────────────
        var serviceAssignments = await context.ServiceEmployeeAssignments
            .AsNoTracking()
            .Where(a =>
                employeeIds.Contains(a.EmployeeId) &&
                a.TransactionService.Transaction.Status == TransactionStatus.Completed &&
                a.TransactionService.Transaction.CompletedAt >= fromUtc &&
                a.TransactionService.Transaction.CompletedAt < toUtc)
            .Select(a => new
            {
                a.EmployeeId,
                a.CommissionAmount,
                Revenue = a.TransactionService.UnitPrice,
            })
            .ToListAsync(cancellationToken);

        var packageAssignments = await context.PackageEmployeeAssignments
            .AsNoTracking()
            .Where(a =>
                employeeIds.Contains(a.EmployeeId) &&
                a.TransactionPackage.Transaction.Status == TransactionStatus.Completed &&
                a.TransactionPackage.Transaction.CompletedAt >= fromUtc &&
                a.TransactionPackage.Transaction.CompletedAt < toUtc)
            .Select(a => new
            {
                a.EmployeeId,
                a.CommissionAmount,
                Revenue = a.TransactionPackage.UnitPrice,
            })
            .ToListAsync(cancellationToken);

        // ── Attendance in range ─────────────────────────────────────────────
        var attendanceRows = await context.Attendances
            .AsNoTracking()
            .Where(a =>
                employeeIds.Contains(a.EmployeeId) &&
                a.Date >= request.From &&
                a.Date <= request.To)
            .Select(a => new { a.EmployeeId, a.TimeIn })
            .ToListAsync(cancellationToken);

        // Total working days in range (approximate: count weekdays)
        var totalDaysInRange = Enumerable
            .Range(0, request.To.DayNumber - request.From.DayNumber + 1)
            .Select(d => request.From.AddDays(d))
            .Count(d => d.DayOfWeek is not (DayOfWeek.Sunday));

        // ── Build rankings ──────────────────────────────────────────────────
        var serviceByEmp = serviceAssignments
            .GroupBy(a => a.EmployeeId)
            .ToDictionary(g => g.Key, g => (Count: g.Count(), Revenue: g.Sum(a => a.Revenue), Commission: g.Sum(a => a.CommissionAmount)));

        var packageByEmp = packageAssignments
            .GroupBy(a => a.EmployeeId)
            .ToDictionary(g => g.Key, g => (Count: g.Count(), Revenue: g.Sum(a => a.Revenue), Commission: g.Sum(a => a.CommissionAmount)));

        var attendanceByEmp = attendanceRows
            .GroupBy(a => a.EmployeeId)
            .ToDictionary(g => g.Key, g => (DaysWorked: g.Count(), DaysLate: 0));

        var rankings = employees
            .Select(emp =>
            {
                var svc = serviceByEmp.GetValueOrDefault(emp.Id);
                var pkg = packageByEmp.GetValueOrDefault(emp.Id);
                var att = attendanceByEmp.GetValueOrDefault(emp.Id);

                var servicesPerformed = svc.Count + pkg.Count;
                var revenueGenerated = svc.Revenue + pkg.Revenue;
                var commissionsEarned = svc.Commission + pkg.Commission;
                var daysWorked = att.DaysWorked;
                var daysLate = att.DaysLate;
                var avgRevenue = servicesPerformed > 0
                    ? Math.Round(revenueGenerated / servicesPerformed, 2)
                    : 0m;
                var attendanceRate = totalDaysInRange > 0
                    ? Math.Round((decimal)daysWorked / totalDaysInRange * 100, 1)
                    : 0m;

                return new EmployeeRankingDto(
                    emp.Id,
                    emp.Name,
                    emp.BranchName,
                    emp.EmployeeType.ToString(),
                    servicesPerformed,
                    revenueGenerated,
                    commissionsEarned,
                    daysWorked,
                    daysLate,
                    avgRevenue,
                    attendanceRate);
            })
            .OrderByDescending(r => r.RevenueGenerated)
            .ToList();

        return new EmployeePerformanceDto(
            request.From,
            request.To,
            request.BranchId,
            rankings.Count,
            rankings.Sum(r => r.CommissionsEarned),
            rankings.Sum(r => r.ServicesPerformed),
            rankings);
    }
}
