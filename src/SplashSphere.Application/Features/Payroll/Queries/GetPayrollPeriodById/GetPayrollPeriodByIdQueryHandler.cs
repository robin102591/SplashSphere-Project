using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Payroll.Queries.GetPayrollPeriodById;

public sealed class GetPayrollPeriodByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPayrollPeriodByIdQuery, PayrollPeriodDetailDto?>
{
    public async Task<PayrollPeriodDetailDto?> Handle(
        GetPayrollPeriodByIdQuery request,
        CancellationToken cancellationToken)
    {
        // ── Query 1: period scalar fields ─────────────────────────────────────
        var period = await context.PayrollPeriods
            .AsNoTracking()
            .Where(p => p.Id == request.PeriodId)
            .Select(p => new
            {
                p.Id,
                p.Status,
                p.Year,
                p.CutOffWeek,
                p.StartDate,
                p.EndDate,
                p.ScheduledReleaseDate,
                p.ReleasedAt,
                p.CreatedAt,
                p.UpdatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (period is null)
            return null;

        // ── Query 2: entries with employee + branch names ─────────────────────
        var entries = await context.PayrollEntries
            .AsNoTracking()
            .Where(e => e.PayrollPeriodId == request.PeriodId)
            .OrderBy(e => e.Employee.LastName)
            .ThenBy(e => e.Employee.FirstName)
            .Select(e => new PayrollEntryDto(
                e.Id,
                e.EmployeeId,
                e.Employee.FirstName + " " + e.Employee.LastName,
                e.Employee.Branch.Name,
                e.EmployeeTypeSnapshot,
                e.DaysWorked,
                e.DailyRateSnapshot,
                e.BaseSalary,
                e.TotalCommissions,
                e.TotalTips,
                e.Bonuses,
                e.Deductions,
                e.BaseSalary + e.TotalCommissions + e.Bonuses - e.Deductions, // NetPay
                e.Notes))
            .ToListAsync(cancellationToken);

        return new PayrollPeriodDetailDto(
            period.Id,
            period.Status,
            period.Year,
            period.CutOffWeek,
            period.StartDate,
            period.EndDate,
            period.ScheduledReleaseDate,
            period.ReleasedAt,
            period.CreatedAt,
            period.UpdatedAt,
            entries);
    }
}
