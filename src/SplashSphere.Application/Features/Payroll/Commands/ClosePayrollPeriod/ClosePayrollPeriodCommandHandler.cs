using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.ClosePayrollPeriod;

public sealed class ClosePayrollPeriodCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IEventPublisher eventPublisher)
    : IRequestHandler<ClosePayrollPeriodCommand, Result>
{
    // Asia/Manila UTC offset — used to convert period DateOnly boundaries to UTC
    // for transaction timestamp comparisons (transactions store CreatedAt as UTC).
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<Result> Handle(
        ClosePayrollPeriodCommand request,
        CancellationToken cancellationToken)
    {
        // ── Load and validate the period ──────────────────────────────────────
        var period = await context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == request.PeriodId, cancellationToken);

        if (period is null)
            return Result.Failure(Error.NotFound("PayrollPeriod", request.PeriodId));

        if (period.Status != PayrollStatus.Open)
            return Result.Failure(Error.Validation(
                $"Only Open periods can be closed. Current status: '{period.Status}'."));

        // ── UTC boundaries for completed-transaction lookups ──────────────────
        // PayrollPeriod dates are Manila calendar dates; transactions store UTC timestamps.
        var periodFromUtc = DateTime.SpecifyKind(period.StartDate.ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);
        var periodToUtc   = DateTime.SpecifyKind(period.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);

        // ── Bulk-load per-employee aggregates (3 queries) ─────────────────────

        // 1. All active employees for the tenant
        var employees = await context.Employees
            .AsNoTracking()
            .Where(e => e.IsActive)
            .Select(e => new
            {
                e.Id,
                e.EmployeeType,
                e.DailyRate,
                e.FirstName,
                e.LastName,
                BranchId = e.BranchId,
            })
            .ToListAsync(cancellationToken);

        // 2. Commission totals per employee from Completed transactions in the period.
        //    Uses CompletedAt so the commission lands in the period the service was paid.
        var commissionsByEmployee = await context.TransactionEmployees
            .AsNoTracking()
            .Where(te =>
                te.Transaction.Status == TransactionStatus.Completed &&
                te.Transaction.CompletedAt >= periodFromUtc &&
                te.Transaction.CompletedAt < periodToUtc)
            .GroupBy(te => te.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, Total = g.Sum(te => te.TotalCommission) })
            .ToListAsync(cancellationToken);

        var commissionsMap = commissionsByEmployee.ToDictionary(x => x.EmployeeId, x => x.Total);

        // 3. Attendance day counts per employee for the period.
        //    Attendance.Date is already a Manila-calendar DateOnly — compare directly.
        var attendanceByEmployee = await context.Attendances
            .AsNoTracking()
            .Where(a =>
                a.Date >= period.StartDate &&
                a.Date <= period.EndDate)
            .GroupBy(a => a.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, Days = g.Count() })
            .ToListAsync(cancellationToken);

        var attendanceMap = attendanceByEmployee.ToDictionary(x => x.EmployeeId, x => x.Days);

        // 4. Tip splits per employee from Completed transactions with tips.
        //    Transaction.TipAmount is split equally among all assigned employees.
        //    Tips are cashed out immediately — tracked here for reporting only.
        var tipRows = await context.TransactionEmployees
            .AsNoTracking()
            .Where(te =>
                te.Transaction.Status == TransactionStatus.Completed &&
                te.Transaction.CompletedAt >= periodFromUtc &&
                te.Transaction.CompletedAt < periodToUtc &&
                te.Transaction.TipAmount > 0)
            .Select(te => new { te.EmployeeId, te.TransactionId, te.Transaction.TipAmount })
            .ToListAsync(cancellationToken);

        var tipsMap = tipRows
            .GroupBy(x => x.TransactionId)
            .SelectMany(txGroup =>
            {
                var count = txGroup.Count();
                var share = Math.Round(txGroup.First().TipAmount / count, 2, MidpointRounding.AwayFromZero);
                return txGroup.Select(x => new { x.EmployeeId, TipShare = share });
            })
            .GroupBy(x => x.EmployeeId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.TipShare));

        // ── Build PayrollEntry rows ────────────────────────────────────────────
        // Only create entries for employees who have actual work this period —
        // at least 1 day worked OR at least ₱0.01 in commissions or tips.
        var entries = new List<PayrollEntry>();

        foreach (var emp in employees)
        {
            var daysWorked   = attendanceMap.GetValueOrDefault(emp.Id, 0);
            var commissions  = commissionsMap.GetValueOrDefault(emp.Id, 0m);
            var tips         = tipsMap.GetValueOrDefault(emp.Id, 0m);

            if (daysWorked == 0 && commissions == 0m && tips == 0m)
                continue;

            var baseSalary = emp.EmployeeType is EmployeeType.Daily or EmployeeType.Hybrid && emp.DailyRate.HasValue
                ? emp.DailyRate.Value * daysWorked
                : 0m;

            var entry = new PayrollEntry(
                tenantContext.TenantId,
                request.PeriodId,
                emp.Id,
                emp.EmployeeType,
                daysWorked,
                emp.EmployeeType is EmployeeType.Daily or EmployeeType.Hybrid ? emp.DailyRate : null,
                baseSalary,
                commissions,
                tips);

            entries.Add(entry);
            context.PayrollEntries.Add(entry);
        }

        // ── Auto-deduct active cash advances (FIFO) ─────────────────────────
        var activeAdvances = await context.CashAdvances
            .Where(ca => ca.Status == CashAdvanceStatus.Active && ca.RemainingBalance > 0)
            .OrderBy(ca => ca.CreatedAt)
            .ToListAsync(cancellationToken);

        var advancesByEmployee = activeAdvances
            .GroupBy(ca => ca.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var entry in entries)
        {
            if (!advancesByEmployee.TryGetValue(entry.EmployeeId, out var empAdvances))
                continue;

            foreach (var advance in empAdvances)
            {
                var deductAmount = Math.Min(advance.DeductionPerPeriod, advance.RemainingBalance);
                advance.RemainingBalance -= deductAmount;

                var adjustment = new PayrollAdjustment(
                    tenantContext.TenantId,
                    entry.Id,
                    AdjustmentType.Deduction,
                    "Cash Advance",
                    deductAmount,
                    $"CA #{advance.Id[..8]}… — ₱{deductAmount:N2} of ₱{advance.Amount:N2}");

                context.PayrollAdjustments.Add(adjustment);

                if (advance.RemainingBalance == 0m)
                    advance.Status = CashAdvanceStatus.FullyPaid;
            }

            entry.RecalculateTotals();
        }

        // ── Transition period to Closed ───────────────────────────────────────
        period.Status = PayrollStatus.Closed;

        eventPublisher.Enqueue(new PayrollPeriodClosedEvent(
            period.Id,
            tenantContext.TenantId,
            period.Year,
            period.CutOffWeek,
            period.StartDate,
            period.EndDate,
            entries.Count));

        return Result.Success();
    }
}
