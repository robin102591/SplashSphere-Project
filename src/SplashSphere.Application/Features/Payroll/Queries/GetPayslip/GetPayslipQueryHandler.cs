using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Payroll.Queries.GetPayslip;

public sealed class GetPayslipQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPayslipQuery, PayslipDto?>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<PayslipDto?> Handle(
        GetPayslipQuery request,
        CancellationToken cancellationToken)
    {
        var entry = await context.PayrollEntries
            .AsNoTracking()
            .Include(e => e.Employee).ThenInclude(emp => emp.Branch).ThenInclude(b => b.Tenant)
            .Include(e => e.PayrollPeriod)
            .Include(e => e.Adjustments)
            .FirstOrDefaultAsync(e => e.Id == request.EntryId, cancellationToken);

        if (entry is null) return null;

        var period = entry.PayrollPeriod;

        if (period.Status is not (PayrollStatus.Closed or PayrollStatus.Processed))
            return null;

        // ── Period label ───────────────────────────────────────────────────────
        var days = period.EndDate.DayNumber - period.StartDate.DayNumber + 1;
        string periodLabel;
        if (days <= 7)
        {
            periodLabel = $"{period.Year} — Week {period.CutOffWeek}";
        }
        else
        {
            var half = period.StartDate.Day <= 15 ? "1st Half" : "2nd Half";
            periodLabel = $"{period.StartDate:MMMM yyyy} — {half}";
        }

        // ── Employee type label ────────────────────────────────────────────────
        var empTypeLabel = entry.EmployeeTypeSnapshot switch
        {
            EmployeeType.Commission => "Commission",
            EmployeeType.Daily => "Daily",
            EmployeeType.Hybrid => "Hybrid",
            _ => entry.EmployeeTypeSnapshot.ToString()
        };

        // ── Adjustment breakdown ───────────────────────────────────────────────
        var bonuses = entry.Adjustments
            .Where(a => a.Type == AdjustmentType.Bonus)
            .Select(a => new PayslipAdjustmentLineDto(a.Category, a.Amount, a.Notes))
            .ToList();

        var deductions = entry.Adjustments
            .Where(a => a.Type == AdjustmentType.Deduction)
            .Select(a => new PayslipAdjustmentLineDto(a.Category, a.Amount, a.Notes))
            .ToList();

        var grossEarnings = entry.BaseSalary + entry.TotalCommissions + entry.TotalTips;

        // ── Commission transaction count ───────────────────────────────────────
        var periodFromUtc = DateTime.SpecifyKind(
            period.StartDate.ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);
        var periodToUtc = DateTime.SpecifyKind(
            period.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);

        var serviceTransactionIds = await context.ServiceEmployeeAssignments
            .AsNoTracking()
            .Where(sea =>
                sea.EmployeeId == entry.EmployeeId &&
                sea.TransactionService.Transaction.Status == TransactionStatus.Completed &&
                sea.TransactionService.Transaction.CompletedAt >= periodFromUtc &&
                sea.TransactionService.Transaction.CompletedAt < periodToUtc)
            .Select(sea => sea.TransactionService.TransactionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var packageTransactionIds = await context.PackageEmployeeAssignments
            .AsNoTracking()
            .Where(pea =>
                pea.EmployeeId == entry.EmployeeId &&
                pea.TransactionPackage.Transaction.Status == TransactionStatus.Completed &&
                pea.TransactionPackage.Transaction.CompletedAt >= periodFromUtc &&
                pea.TransactionPackage.Transaction.CompletedAt < periodToUtc)
            .Select(pea => pea.TransactionPackage.TransactionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var commissionTransactions = serviceTransactionIds
            .Union(packageTransactionIds)
            .Distinct()
            .Count();

        return new PayslipDto(
            entry.Employee.Branch.Tenant.Name,
            entry.Employee.Branch.Name,
            periodLabel,
            period.StartDate,
            period.EndDate,
            $"{entry.Employee.FirstName} {entry.Employee.LastName}",
            empTypeLabel,
            entry.EmployeeId,
            entry.BaseSalary,
            entry.TotalCommissions,
            entry.TotalTips,
            grossEarnings,
            bonuses,
            deductions,
            entry.Bonuses,
            entry.Deductions,
            entry.NetPay,
            entry.DaysWorked,
            commissionTransactions,
            DateTime.UtcNow);
    }
}
