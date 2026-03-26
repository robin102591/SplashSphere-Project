using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Payroll.Queries.GetPayrollEntryDetail;

public sealed class GetPayrollEntryDetailQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPayrollEntryDetailQuery, PayrollEntryDetailDto?>
{
    private static readonly TimeSpan ManilaOffset = TimeSpan.FromHours(8);

    public async Task<PayrollEntryDetailDto?> Handle(
        GetPayrollEntryDetailQuery request,
        CancellationToken cancellationToken)
    {
        // ── Load entry with employee + period ────────────────────────────────
        var entry = await context.PayrollEntries
            .AsNoTracking()
            .Include(e => e.Employee)
                .ThenInclude(emp => emp.Branch)
            .Include(e => e.PayrollPeriod)
            .FirstOrDefaultAsync(e => e.Id == request.EntryId, cancellationToken);

        if (entry is null) return null;

        var entryDto = new PayrollEntryDto(
            entry.Id,
            entry.EmployeeId,
            $"{entry.Employee.FirstName} {entry.Employee.LastName}",
            entry.Employee.Branch.Name,
            entry.EmployeeTypeSnapshot,
            entry.DaysWorked,
            entry.DailyRateSnapshot,
            entry.BaseSalary,
            entry.TotalCommissions,
            entry.TotalTips,
            entry.Bonuses,
            entry.Deductions,
            entry.NetPay,
            entry.Notes);

        // ── UTC boundaries (same logic as ClosePayrollPeriodCommandHandler) ──
        var period = entry.PayrollPeriod;
        var periodFromUtc = DateTime.SpecifyKind(
            period.StartDate.ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);
        var periodToUtc = DateTime.SpecifyKind(
            period.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue) - ManilaOffset, DateTimeKind.Utc);

        // ── Commission line items from services ──────────────────────────────
        var serviceCommissions = await context.ServiceEmployeeAssignments
            .AsNoTracking()
            .Where(sea =>
                sea.EmployeeId == entry.EmployeeId &&
                sea.TransactionService.Transaction.Status == TransactionStatus.Completed &&
                sea.TransactionService.Transaction.CompletedAt >= periodFromUtc &&
                sea.TransactionService.Transaction.CompletedAt < periodToUtc)
            .Select(sea => new CommissionLineItemDto(
                sea.TransactionService.Transaction.TransactionNumber,
                sea.TransactionService.Service.Name,
                sea.CommissionAmount,
                sea.TransactionService.Transaction.CompletedAt!.Value))
            .ToListAsync(cancellationToken);

        // ── Commission line items from packages ──────────────────────────────
        var packageCommissions = await context.PackageEmployeeAssignments
            .AsNoTracking()
            .Where(pea =>
                pea.EmployeeId == entry.EmployeeId &&
                pea.TransactionPackage.Transaction.Status == TransactionStatus.Completed &&
                pea.TransactionPackage.Transaction.CompletedAt >= periodFromUtc &&
                pea.TransactionPackage.Transaction.CompletedAt < periodToUtc)
            .Select(pea => new CommissionLineItemDto(
                pea.TransactionPackage.Transaction.TransactionNumber,
                pea.TransactionPackage.Package.Name,
                pea.CommissionAmount,
                pea.TransactionPackage.Transaction.CompletedAt!.Value))
            .ToListAsync(cancellationToken);

        var allCommissions = serviceCommissions
            .Concat(packageCommissions)
            .OrderByDescending(c => c.CompletedAt)
            .ToList();

        // ── Attendance records ───────────────────────────────────────────────
        var attendance = await context.Attendances
            .AsNoTracking()
            .Where(a =>
                a.EmployeeId == entry.EmployeeId &&
                a.Date >= period.StartDate &&
                a.Date <= period.EndDate)
            .OrderBy(a => a.Date)
            .Select(a => new AttendanceLineItemDto(a.Date, a.TimeIn, a.TimeOut))
            .ToListAsync(cancellationToken);

        // ── Adjustment line items ──────────────────────────────────────────────
        var adjustments = await context.PayrollAdjustments
            .AsNoTracking()
            .Where(a => a.PayrollEntryId == entry.Id)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new PayrollAdjustmentDto(
                a.Id, a.Type, a.Category, a.Amount, a.Notes,
                a.TemplateId, a.Template != null ? a.Template.Name : null,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PayrollEntryDetailDto(entryDto, allCommissions, attendance, adjustments);
    }
}
