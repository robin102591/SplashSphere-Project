using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Events;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Payroll.Commands.ProcessPayrollPeriod;

public sealed class ProcessPayrollPeriodCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IEventPublisher eventPublisher)
    : IRequestHandler<ProcessPayrollPeriodCommand, Result>
{
    public async Task<Result> Handle(
        ProcessPayrollPeriodCommand request,
        CancellationToken cancellationToken)
    {
        var period = await context.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == request.PeriodId, cancellationToken);

        if (period is null)
            return Result.Failure(Error.NotFound("PayrollPeriod", request.PeriodId));

        if (period.Status != PayrollStatus.Closed)
            return Result.Failure(Error.Validation(
                $"Only Closed periods can be processed. Current status: '{period.Status}'."));

        // Compute total net pay for the event payload — sum across all entries.
        var totalNetPay = await context.PayrollEntries
            .AsNoTracking()
            .Where(e => e.PayrollPeriodId == request.PeriodId)
            .SumAsync(e => e.BaseSalary + e.TotalCommissions + e.Bonuses - e.Deductions,
                cancellationToken);

        period.Status = PayrollStatus.Processed;

        // Resolve effective settings: branch override → tenant default
        var settings = period.BranchId is not null
            ? await context.PayrollSettings.AsNoTracking()
                .FirstOrDefaultAsync(s => s.BranchId == period.BranchId, cancellationToken)
              ?? await context.PayrollSettings.AsNoTracking()
                .FirstOrDefaultAsync(s => s.TenantId == tenantContext.TenantId && s.BranchId == null, cancellationToken)
            : await context.PayrollSettings.AsNoTracking()
                .FirstOrDefaultAsync(s => s.TenantId == tenantContext.TenantId && s.BranchId == null, cancellationToken);

        var offset = settings?.PayReleaseDayOffset ?? 3;
        if (offset > 0)
            period.ScheduledReleaseDate = period.EndDate.AddDays(offset);

        eventPublisher.Enqueue(new PayrollProcessedEvent(
            period.Id,
            tenantContext.TenantId,
            period.BranchId,
            period.Year,
            period.CutOffWeek,
            totalNetPay));

        return Result.Success();
    }
}
